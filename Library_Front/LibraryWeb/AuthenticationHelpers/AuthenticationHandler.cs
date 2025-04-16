using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using DataModelLibrary.AuthRequestModels;
using Microsoft.AspNetCore.Components;

namespace LibraryWeb.Authentication
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;
        private readonly string _apiBaseUrl = "https://localhost:7121/";

        public AuthenticationHandler(
            ILocalStorageService localStorage,
            NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                if (IsTokenExpired(token))
                {
                    var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var refreshResponse = await TryRefreshToken(token, refreshToken);
                        if (refreshResponse != null)
                        {
                            await _localStorage.SetItemAsync("authToken", refreshResponse.AccessToken);
                            await _localStorage.SetItemAsync("refreshToken", refreshResponse.RefreshToken);
                            token = refreshResponse.AccessToken;
                        }
                        else
                        {
                            await _localStorage.RemoveItemAsync("authToken");
                            await _localStorage.RemoveItemAsync("refreshToken");
                            _navigationManager.NavigateTo("/login");
                            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                        }
                    }
                    else
                    {
                        await _localStorage.RemoveItemAsync("authToken");
                        _navigationManager.NavigateTo("/login");
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    }
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshResponse = await TryRefreshToken(token!, refreshToken);
                    if (refreshResponse != null)
                    {
                        await _localStorage.SetItemAsync("authToken", refreshResponse.AccessToken);
                        await _localStorage.SetItemAsync("refreshToken", refreshResponse.RefreshToken);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResponse.AccessToken);
                        return await base.SendAsync(request, cancellationToken);
                    }
                }

                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                _navigationManager.NavigateTo("/login");
            }

            return response;
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo.Kind != DateTimeKind.Utc)
                {
                    var expiration = DateTime.SpecifyKind(jwtToken.ValidTo, DateTimeKind.Utc);

                    return expiration.AddMinutes(-5) < DateTime.UtcNow;
                }

                return jwtToken.ValidTo.AddMinutes(-5) < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        private async Task<AuthResponse?> TryRefreshToken(string token, string refreshToken)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(_apiBaseUrl)
            };

            try
            {
                var response = await client.PostAsJsonAsync("api/Authentication/RefreshToken",
                    new RefreshTokenRequest
                    {
                        AccessToken = token,
                        RefreshToken = refreshToken
                    });

                return response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<AuthResponse>()
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}