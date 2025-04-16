using DataModelLibrary.AuthRequestModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace LibraryWeb.Pages.Authentication
{
    public partial class Register
    {
        [Inject]
        private HttpClient HttpClient { get; set; } = default!;
        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        private MudForm form;
        private bool success;
        private RegisterRequest registerRequest = new();
        private bool _isLoggedIn = false;
        private bool _showPassword = false;

        // Password requirement flags
        private bool HasMinLength => registerRequest.Password.Length >= 8;
        private bool HasUpperCase => registerRequest.Password.Any(char.IsUpper);
        private bool HasLowerCase => registerRequest.Password.Any(char.IsLower);
        private bool HasDigit => registerRequest.Password.Any(char.IsDigit);

        protected override async Task OnInitializedAsync()
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");
            _isLoggedIn = !string.IsNullOrEmpty(token);

            if (_isLoggedIn)
            {
                NavigationManager.NavigateTo("/");
            }
        }

        private string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Name is required";
            if (name.Length < 2)
                return "Name must be at least 2 characters";
            if (!name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                return "Name can only contain letters";
            return null;
        }

        private string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email is required";
            if (!new EmailAddressAttribute().IsValid(email))
                return "Please enter a valid email address";
            return null;
        }

        private string ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Username is required";
            if (username.Length < 3)
                return "Username must be at least 3 characters";
            if (username.Contains(' '))
                return "Username cannot contain spaces";
            return null;
        }

        private string ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required";

            var errors = new List<string>();
            if (!HasMinLength)
                errors.Add("Must be at least 8 characters");
            if (!HasUpperCase)
                errors.Add("Must contain an uppercase letter");
            if (!HasLowerCase)
                errors.Add("Must contain a lowercase letter");
            if (!HasDigit)
                errors.Add("Must contain a number");

            return errors.Any() ? string.Join(", ", errors) : null;
        }

        private async Task HandleRegister()
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync("api/Authentication/Register", registerRequest);
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authResponse != null)
                    {
                        await AuthStateService.SetAuthenticationState(authResponse.AccessToken, authResponse.RefreshToken);
                        NavigationManager.NavigateTo("/");
                        Snackbar.Add("Registration successful!", Severity.Success);
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
