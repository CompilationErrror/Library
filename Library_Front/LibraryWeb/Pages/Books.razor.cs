using DataModelLibrary.AuthModels;
using DataModelLibrary.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

namespace LibraryWeb.Pages
{
    public partial class Books
    {
        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private ISnackbar _snackbar { get; set; }
        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        private bool _isPageLoading = true;
        private bool _isDataLoading = true;
        private List<Book> _books = new();
        private string _searchString = "";

        private Book? _selectedBook;
        private bool _showDialog;

        private bool _showDeleteDialog;
        private Book _bookToDelete;

        private bool _showAddBookDialog = false;
        private Book _newBook = new Book();
        private bool _isAddingBook = false;

        private bool _showEditBookDialog = false;
        private Book _bookToEdit = new Book();
        private bool _isEditingBook = false;

        private bool _isAuthorized = false;
        private bool _isAdmin;

        private IReadOnlyList<IBrowserFile>? _coverImage;
        private string _bookCoverUrl = "";
        private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;
        private readonly List<string> _fileNames = new();
        private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full";
        private string _dragClass = DefaultDragClass;

        protected override async Task OnInitializedAsync()
        {
            _isAuthorized = await AuthStateService.IsAuthenticated();
            _isPageLoading = false;

            if (_isAuthorized)
            {
                var token = await LocalStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    _isAdmin = jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
                }

                await LoadBooks();
            }

            AuthStateService.AuthenticationChanged += HandleAuthenticationChanged;
        }

        private async Task LoadBooks()
        {
            try
            {
                _books = await HttpClient.GetFromJsonAsync<List<Book>>("GetBooks");
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading books: {ex.Message}", Severity.Error);
                _books = new List<Book>();
            }
            finally 
            {
                _isDataLoading = false;
            }
        }

        private async Task GetBookCover(Book book)
        {
            var response = await HttpClient.GetAsync($"GetCover?id={book.Id}");
            if (response.IsSuccessStatusCode)
            {
                var imageUri = await response.Content.ReadAsStringAsync();
                _bookCoverUrl = imageUri;
            }
            else
            {
                _bookCoverUrl = "";
            }

            _selectedBook = book;
            _showDialog = true;
        }

        private async Task UploadCoverImage()
        {
            if (_coverImage != null && _coverImage.Any())
            {
                var response = await HttpClient.PostAsync($"AddCover?bookId={_selectedBook.Id}", new MultipartFormDataContent
                {
                    { new StreamContent(_coverImage.First().OpenReadStream()), "coverImg", _coverImage.First().Name }
                });

                if (response.IsSuccessStatusCode)
                {
                    _bookCoverUrl = await response.Content.ReadAsStringAsync();
                        
                    _snackbar.Add("Cover image uploaded successfully.", Severity.Success);
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    _snackbar.Add($"Error uploading cover image: {response.StatusCode}", Severity.Error);
                }
            }
        }

        private async Task DeleteCover()
        {
            var response = await HttpClient.DeleteAsync($"DeleteCover?bookId={_selectedBook.Id}");

            if (response.IsSuccessStatusCode)
            {
                _snackbar.Add("Cover image deleted successfully.", Severity.Success);
                _bookCoverUrl = "";
                await InvokeAsync(StateHasChanged);
            }
            else 
            {
                _snackbar.Add("Problem occur while deleting cover image.", Severity.Error);
            }
        }

        private async Task OrderBook() 
        {
            var token = await LocalStorage.GetItemAsync<string>("authToken");

            var tokenResponse = await HttpClient.PostAsync($"api/Authentication/GetTokenInstance?accessToken={Uri.EscapeDataString(token)}", null);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _snackbar.Add("Error retrieving user information", Severity.Error);
                return;
            }

            var userToken = await tokenResponse.Content.ReadFromJsonAsync<UserToken>();

            var orderResponse = await HttpClient.PostAsync($"OrderBook?bookId={_selectedBook.Id}&custId={userToken.UserId}", null);

            if (orderResponse.IsSuccessStatusCode)
            {
                _snackbar.Add("Book ordered successfully", Severity.Success);
                CloseDialog();
                await LoadBooks();
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                var errorMessage = await orderResponse.Content.ReadAsStringAsync();
                _snackbar.Add($"Error ordering book: {errorMessage}", Severity.Error);
            }
        }

        private async Task AddNewBook()
        {
            if (string.IsNullOrWhiteSpace(_newBook.Title) || string.IsNullOrWhiteSpace(_newBook.Author))
            {
                _snackbar.Add("Title and Author are required.", Severity.Warning);
                return;
            }

            _isAddingBook = true;

            var response = await HttpClient.PostAsJsonAsync("AddBook", _newBook);

            if (response.IsSuccessStatusCode)
            {
                _snackbar.Add("Book added successfully.", Severity.Success);
                await LoadBooks();
                _showAddBookDialog = false;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Error adding book: {errorMessage}", Severity.Error);
            }

            _isAddingBook = false;
        }

        private void EditBookClick(Book book)
        {
            _bookToEdit = book;
            _showEditBookDialog = true;
            StateHasChanged();
        }

        private async Task UpdateBook()
        {
            if (string.IsNullOrWhiteSpace(_bookToEdit.Title) || string.IsNullOrWhiteSpace(_bookToEdit.Author))
            {
                _snackbar.Add("Title and Author are required.", Severity.Warning);
                return;
            }

            _isEditingBook = true;

            var response = await HttpClient.PutAsJsonAsync("ChangeBook", _bookToEdit);

            if (response.IsSuccessStatusCode)
            {
                _snackbar.Add("Book updated successfully.", Severity.Success);
                await LoadBooks();
                _showEditBookDialog = false;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Error updating book: {errorMessage}", Severity.Error);
            }

            _isEditingBook = false;
        }
        private void DeleteBookClick(Book book)
        {
            _bookToDelete = book;
            _showDeleteDialog = true;
            StateHasChanged();
        }

        private async Task ConfirmDeleteBook()
        {
            await HttpClient.DeleteAsync($"DeleteCover?bookId={_bookToDelete.Id}");

            var response = await HttpClient.DeleteAsync($"DeleteBook?id={_bookToDelete.Id}");

            if (response.IsSuccessStatusCode)
            {
                _snackbar.Add("Book deleted successfully.", Severity.Success);
                await LoadBooks();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Error deleting book: {errorMessage}", Severity.Error);
            }


            _showDeleteDialog = false;
            _bookToDelete = null;
        }

        private void CancelDelete()
        {
            _showDeleteDialog = false;
            _bookToDelete = null;
        }

        private async void OnRowClick(TableRowClickEventArgs<Book> books)
        {
            await GetBookCover(books.Item);
            StateHasChanged();
        }

        private void OpenAddBookDialog()
        {
            _newBook = new Book(); 
            _showAddBookDialog = true;
        }

        private void CloseDialog()
        {
            _showDialog = false;
            _selectedBook = null;
            _bookCoverUrl = "";
            _coverImage = null;
            ClearDragClass();
        }

        private void CloseAddBookDialog()
        {
            _showAddBookDialog = false;
            _newBook = new Book(); 
            _isAddingBook = false; 
        }

        private void CloseEditBookDialog()
        {
            _showEditBookDialog = false;
            _bookToEdit = new Book();
            _isEditingBook = false;
        }
        private async void HandleAuthenticationChanged()
        {
            _isAuthorized = await AuthStateService.IsAuthenticated();

            if (_isAuthorized)
            {
                var token = await LocalStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    _isAdmin = jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
                }

                await LoadBooks();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task ClearAsync()
        {
            await (_fileUpload?.ClearAsync() ?? Task.CompletedTask);
            _fileNames.Clear();
            ClearDragClass();
        }

        private Task OpenFilePickerAsync()
            => _fileUpload?.OpenFilePickerAsync() ?? Task.CompletedTask;

        private void OnInputFileChanged(InputFileChangeEventArgs e)
        {
            ClearDragClass();
            _coverImage = e.GetMultipleFiles();
            foreach (var file in _coverImage)
            {
                _fileNames.Add(file.Name);
            }
        }
        private void SetDragClass()
            => _dragClass = $"{DefaultDragClass} mud-border-primary";

        private void ClearDragClass()
            => _dragClass = DefaultDragClass;
    }
}