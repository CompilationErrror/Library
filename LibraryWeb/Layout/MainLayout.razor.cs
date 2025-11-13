using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace LibraryWeb.Layout
{
    partial class MainLayout
    {
        private bool _isAuthenticated;
        private bool _isAdmin;

        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _isAuthenticated = await AuthStateService.IsAuthenticated();

            _isAdmin = await AuthStateService.IsAdmin();

            AuthStateService.AuthenticationChanged += HandleAuthenticationChanged;
        }

        private async void HandleAuthenticationChanged()
        {
            _isAuthenticated = await AuthStateService.IsAuthenticated();
            await InvokeAsync(StateHasChanged);
        }

        private async Task HandleLogout()
        {
            try
            {
                var accessToken = await LocalStorage.GetItemAsync<string>("authToken");
                var refreshToken = await LocalStorage.GetItemAsync<string>("refreshToken");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    Http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var response = await Http.PostAsJsonAsync("api/Authentication/Logout", refreshToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Server returned {response.StatusCode}");
                    }
                }

                await AuthStateService.ClearAuthenticationState();
                Snackbar.Add("Logged out successfully!", Severity.Success);
                NavigationManager.NavigateTo("/login");
            }
            catch (Exception ex)
            {
                await AuthStateService.ClearAuthenticationState();
                Snackbar.Add($"Error during logout: {ex.Message}", Severity.Error);
            }
        }
        public void Dispose()
        {
            AuthStateService.AuthenticationChanged -= HandleAuthenticationChanged;
        }
    }
}