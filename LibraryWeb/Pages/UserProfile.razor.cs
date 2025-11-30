using DataModelLibrary.Models;
using LibraryWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LibraryWeb.Pages
{
    public partial class UserProfile
    {
        [Inject]
        private IUserProfileServiceClient UserProfileServiceClient { get; set; } = default!;

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
            var response = await UserProfileServiceClient.GetUserByIdAsync();

            if (response.IsSuccess && response.Data != null)
            {
                _user = response.Data;
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
                _snackbar.Add($"Error loading user data: {response.ErrorMessage}", Severity.Error);
            }
        }

        private async Task LoadUserStatistics()
        {
            var response = await UserProfileServiceClient.GetUserStatsAsync();

            if (response.IsSuccess && response.Data != null)
            {
                var stats = response.Data;
                _currentlyBorrowed = stats.CurrentlyBorrowed;
                _booksReturned = stats.BooksReturned;
                _overdueBooks = stats.OverdueBooks;
            }
            else
            {
                _snackbar.Add($"Error loading user statistics: {response.ErrorMessage}", Severity.Error);
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

            var response = await UserProfileServiceClient.UpdateUserAsync(updateModel);

            if (response.IsSuccess)
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
                _snackbar.Add($"Error updating profile: {response.ErrorMessage}", Severity.Error);
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