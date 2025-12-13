using Blazored.LocalStorage;
using DataModelLibrary.AuthRequestModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly string _baseAddress;

    private const int AccessTokenEarlyRefreshMinutes = 2;
    private static readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

    public AuthenticationHandler(
        ILocalStorageService localStorage,
        NavigationManager navigationManager)
    {
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _baseAddress = "https://localhost:7121/";
    }

    protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
    {
        try
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        }
        catch { }

        if (request.RequestUri?.AbsolutePath.StartsWith("/api/Authentication", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var accessToken = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            if (IsAccessTokenExpiredOrNearExpiry(accessToken))
            {
                var refreshSucceeded = await TryRefreshTokenAsync(accessToken);
                if (!refreshSucceeded)
                {
                    await ClearTokensAndRedirect();
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                accessToken = await _localStorage.GetItemAsync<string>("authToken");
            }

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await ClearTokensAndRedirect();
        }

        return response;
    }

    private bool IsAccessTokenExpiredOrNearExpiry(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            return jwtToken.ValidTo.AddMinutes(-AccessTokenEarlyRefreshMinutes) <= DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    private async Task<bool> TryRefreshTokenAsync(string currentAccessToken)
    {
        await _refreshLock.WaitAsync();
        try
        {
            var latestAccessToken = await _localStorage.GetItemAsync<string>("authToken");
            if (latestAccessToken != currentAccessToken && !string.IsNullOrWhiteSpace(latestAccessToken))
            {
                if (!IsAccessTokenExpiredOrNearExpiry(latestAccessToken))
                    return true;
            }

            using var httpClient = new HttpClient { BaseAddress = new Uri(_baseAddress) };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/Authentication/RefreshToken");

            try
            {
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            }
            catch { }

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return false;

            var refreshResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (refreshResponse == null)
                return false;

            await _localStorage.SetItemAsync("authToken", refreshResponse.AccessToken);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task ClearTokensAndRedirect()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }
}