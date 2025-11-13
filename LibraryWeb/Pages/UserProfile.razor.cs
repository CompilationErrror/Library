using DataModelLibrary.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace LibraryWeb.Pages
{
    public partial class UserProfile
    {
        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private ISnackbar _snackbar { get; set; }

        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        private bool _isPageLoading = true;
        private bool _isAuthorized = false;
        private User _user = new();
        private User _userEdit = new();
        private bool _editMode = false;
        private bool _isValid = false;
        private bool _isSaving = false;
        private MudForm _form;

        private string _currentPassword = "";
        private string _newPassword = "";
        private string _confirmPassword = "";
        private bool _showConfirmDialog = false;

        private string _originalName = "";
        private string _originalSurname = "";
        private string _originalEmail = "";

        private int _currentlyBorrowed = 0;
        private int _booksReturned = 0;
        private int _overdueBooks = 0;

        protected override async Task OnInitializedAsync()
        {
            _isAuthorized = await AuthStateService.IsAuthenticated();

            if (_isAuthorized)
            {
                await LoadUserData();
                await LoadUserStatistics();
            }

            _isPageLoading = false;
        }

        private async Task LoadUserData()
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var userResponse = await HttpClient.GetAsync($"GetUserById");

                if (userResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        _user = await userResponse.Content.ReadFromJsonAsync<User>();
                    }
                    catch (Exception ex)
                    {
                        _snackbar.Add($"Error loading user data: {ex.Message}", Severity.Error);
                    }
                    _userEdit = new User
                    {
                        Id = _user.Id,
                        Name = _user.Name,
                        Surname = _user.Surname,
                        Email = _user.Email,
                        Username = _user.Username,
                        IsAdmin = _user.IsAdmin
                    };

                    _originalName = _user.Name;
                    _originalSurname = _user.Surname;
                    _originalEmail = _user.Email;
                }
                else
                {
                    var errorMessage = await userResponse.Content.ReadAsStringAsync();
                    _snackbar.Add($"Error loading user data: {errorMessage}", Severity.Error);
                }
            }
        }

        private async Task LoadUserStatistics()
        {
            var statsResponse = await HttpClient.GetAsync($"GetUserStats");

            if (statsResponse.IsSuccessStatusCode)
            {
                var stats = await statsResponse.Content.ReadFromJsonAsync<UserStats>();
                _currentlyBorrowed = stats.CurrentlyBorrowed;
                _booksReturned = stats.BooksReturned;
                _overdueBooks = stats.OverdueBooks;
            }
        }

        private void CancelEdit()
        {
            _editMode = false;
            _currentPassword = "";
            _newPassword = "";
            _confirmPassword = "";

            _userEdit = new User
            {
                Id = _user.Id,
                Name = _user.Name,
                Surname = _user.Surname,
                Email = _user.Email,
                Username = _user.Username,
                IsAdmin = _user.IsAdmin
            };
        }

        private async Task SaveProfile()
        {
            await _form.Validate();

            if (!_isValid)
            {
                return;
            }

            _isSaving = true;

            try
            {
                if (!string.IsNullOrEmpty(_currentPassword) && !string.IsNullOrEmpty(_newPassword))
                {
                    _showConfirmDialog = true;
                    return;
                }

                await UpdateUserProfile(false);
            }
            finally
            {
                _isSaving = false;
            }
        }

        private async Task ConfirmPasswordChange()
        {
            _showConfirmDialog = false;
            _isSaving = true;

            await UpdateUserProfile(true);

            _isSaving = false;            
        }

        private async Task UpdateUserProfile(bool includePassword)
        {

            var updateModel = new UserUpdateModel
            {
                UserId = _userEdit.Id
            };

            if (_userEdit.Name != _originalName)
                updateModel.Name = _userEdit.Name;

            if (_userEdit.Surname != _originalSurname)
                updateModel.Surname = _userEdit.Surname;

            if (_userEdit.Email != _originalEmail)
                updateModel.Email = _userEdit.Email;

            if (includePassword)
            {
                updateModel.CurrentPassword = _currentPassword;
                updateModel.NewPassword = _newPassword;
            }

            var response = await HttpClient.PutAsJsonAsync("UpdateUser", updateModel);

            if (response.IsSuccessStatusCode)
            {
                _snackbar.Add("Profile updated successfully", Severity.Success);

                _user.Name = _userEdit.Name;
                _user.Surname = _userEdit.Surname;
                _user.Email = _userEdit.Email;

                _originalName = _userEdit.Name;
                _originalSurname = _userEdit.Surname;
                _originalEmail = _userEdit.Email;

                _currentPassword = "";
                _newPassword = "";
                _confirmPassword = "";

                _editMode = false;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Error updating profile: {errorMessage}", Severity.Error);
            }

            StateHasChanged();
        }

        private string PasswordMatch(string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(_newPassword) && string.IsNullOrWhiteSpace(confirmPassword))
                return null;

            if (_newPassword != confirmPassword)
                return "Passwords do not match";

            return null;
        }
    }
}