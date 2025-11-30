using DataModelLibrary.AuthRequestModels;
using LibraryWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LibraryWeb.Pages.Authentication
{
    public partial class Login
    {
        [Inject]
        private IAuthenticationServiceClient AuthService { get; set; } = default!;

        private MudForm form;
        private bool success;
        private LoginRequest loginRequest = new();
        private bool _isLoggedIn = false;

        protected override async Task OnInitializedAsync()
        {
            _isLoggedIn = await AuthService.IsAuthenticatedAsync();

            if (_isLoggedIn)
            {
                NavigationManager.NavigateTo("/");
            }
        }

        private async Task HandleLogin()
        {
            var result = await AuthService.LoginAsync(loginRequest);
            
            if (result.IsSuccess)
            {
                NavigationManager.NavigateTo("/");
                Snackbar.Add("Login successful!", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.ErrorMessage, Severity.Error);
            }
        }
    }
}