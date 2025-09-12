using Microsoft.AspNetCore.Components;
using MudBlazor;

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
                var token = await LocalStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    Http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var response = await Http.PostAsync("api/Authentication/Logout", null);

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
                Snackbar.Add($"Error during logout: {ex.Message}", Severity.Error);
            }
        }
        public void Dispose()
        {
            AuthStateService.AuthenticationChanged -= HandleAuthenticationChanged;
        }
    }
}