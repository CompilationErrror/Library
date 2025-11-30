using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

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
                if (!string.IsNullOrEmpty(accessToken))
                {
                    Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    using var request = new HttpRequestMessage(HttpMethod.Post, "api/Authentication/Logout");
                    try
                    {
                        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                    }
                    catch
                    {

                    }

                    var response = await Http.SendAsync(request);

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