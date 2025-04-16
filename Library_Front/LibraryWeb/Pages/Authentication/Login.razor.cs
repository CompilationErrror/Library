using DataModelLibrary.AuthRequestModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace LibraryWeb.Pages.Authentication
{
    public partial class Login
    {
        [Inject]
        private HttpClient HttpClient { get; set; } = default!;
        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        private MudForm form;
        private bool success;
        private LoginRequest loginRequest = new();
        private bool _isLoggedIn = false;

        protected override async Task OnInitializedAsync()
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");
            _isLoggedIn = !string.IsNullOrEmpty(token);

            if (_isLoggedIn)
            {
                NavigationManager.NavigateTo("/");
            }
        }

        private async Task HandleLogin()
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync("api/Authentication/Login", loginRequest);
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authResponse != null)
                    {
                        await AuthStateService.SetAuthenticationState(authResponse.AccessToken, authResponse.RefreshToken);
                        NavigationManager.NavigateTo("/");
                        Snackbar.Add("Login successful!", Severity.Success);
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add(error, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
        }
    }
}
