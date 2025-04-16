﻿using System.Net.Http.Headers;
using Blazored.LocalStorage;
using System.Net.Http.Json;

public class AuthenticationStateService
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://localhost:7121/";

    private bool? _isAuthenticated;
    private DateTime _lastValidated = DateTime.MinValue;
    private TimeSpan _validationInterval = TimeSpan.FromHours(1); 

    public event Action? AuthenticationChanged;

    public AuthenticationStateService(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public async Task SetAuthenticationState(string accessToken, string refreshToken)
    {
        await _localStorage.SetItemAsync("authToken", accessToken);
        await _localStorage.SetItemAsync("refreshToken", refreshToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Update cached state
        _isAuthenticated = true;
        _lastValidated = DateTime.UtcNow;

        NotifyAuthenticationChanged();
    }

    public async Task ClearAuthenticationState()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
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
            return false;
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
        return true;
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