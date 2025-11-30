using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

public class AuthenticationStateService
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://localhost:7121/";

    private bool? _isAuthenticated;
    private DateTime _lastValidated = DateTime.MinValue;
    private TimeSpan _validationInterval = TimeSpan.FromMinutes(30);

    public event Action? AuthenticationChanged;

    public AuthenticationStateService(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public async Task SetAuthenticationState(string accessToken)
    {
        await _localStorage.SetItemAsync("authToken", accessToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        _isAuthenticated = true;
        _lastValidated = DateTime.UtcNow;

        NotifyAuthenticationChanged();
    }

    public async Task ClearAuthenticationState()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        _isAuthenticated = false;

        NotifyAuthenticationChanged();
    }

    public async Task<bool> IsAuthenticated()
    {
        if (_isAuthenticated.HasValue && DateTime.UtcNow - _lastValidated < _validationInterval)
        {
            return _isAuthenticated.Value;
        }

        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token))
        {
            _isAuthenticated = false;
            return _isAuthenticated.Value;
        }

        if (DateTime.UtcNow - _lastValidated >= _validationInterval)
        {
            if (!await ValidateTokenWithServer(token))
            {
                await ClearAuthenticationState();
                return false;
            }

            _lastValidated = DateTime.UtcNow;
        }

        _isAuthenticated = true;
        return _isAuthenticated.Value;
    }

    public async Task<bool> IsAdmin()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        return false;
    }

    private async Task<bool> ValidateTokenWithServer(string token)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(_apiBaseUrl)
        };

        try
        {
            var response = await client.PostAsJsonAsync("api/Authentication/ValidateToken", token);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private void NotifyAuthenticationChanged()
    {
        AuthenticationChanged?.Invoke();
    }
}