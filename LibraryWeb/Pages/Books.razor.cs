using DataModelLibrary.Models;
using LibraryWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace LibraryWeb.Pages
{
    public partial class Books
    {
        [Inject]
        private IBookServiceClient BookService { get; set; } = default!;

        [Inject]
        private ISnackbar _snackbar { get; set; }

        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        private bool _isPageLoading = true;
        private bool _isDataLoading = true;
        private List<Book> _books = new();

        private MudTable<Book> _table;

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

        private int _currentOffset = 0;
        private readonly int _pageSize = 10;
        private readonly int _totalCount = 0;
        private int _currentPage = 1;
        private readonly int _totalPages = 1;

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
                _isAdmin = await AuthStateService.IsAdmin();
            }

            AuthStateService.AuthenticationChanged += HandleAuthenticationChanged;
        }

        private async Task<TableData<Book>> LoadBooks(TableState state, CancellationToken cancellationToken)
        {
            _isDataLoading = true;

            var offset = state.Page * state.PageSize;
            var pageSize = state.PageSize;

            string? sortBy = state.SortLabel ?? "Id"; 
            bool sortDescending = state.SortDirection == SortDirection.Descending;

            var result = await BookService.GetBooksAsync(offset, pageSize, sortBy, sortDescending);

            _isDataLoading = false;

            if (!result.IsSuccess || result.Data == null)
            {
                return new TableData<Book>
                {
                    Items = Array.Empty<Book>(),
                    TotalItems = 0
                };
            }

            return new TableData<Book>
            {
                Items = result.Data.Items,
                TotalItems = result.Data.TotalCount
            };
        }

        private async Task GetBookCover(Book book)
        {
            var result = await BookService.GetBookCoverAsync(book.Id);

            _bookCoverUrl = result.IsSuccess ? result.Data : "";
            _selectedBook = book;
            _showDialog = true;
        }

        private async Task UploadCoverImage()
        {
            if (_coverImage != null && _coverImage.Any())
            {
                var result = await BookService.UploadBookCoverAsync(_selectedBook.Id, _coverImage.First());

                if (result.IsSuccess)
                {
                    _bookCoverUrl = result.Data;
                    _snackbar.Add("Cover image uploaded successfully.", Severity.Success);
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    _snackbar.Add(result.ErrorMessage, Severity.Error);
                }
            }
        }

        private async Task DeleteCover()
        {
            var result = await BookService.DeleteBookCoverAsync(_selectedBook.Id);

            if (result.IsSuccess)
            {
                _snackbar.Add("Cover image deleted successfully.", Severity.Success);
                _bookCoverUrl = "";
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                _snackbar.Add(result.ErrorMessage, Severity.Error);
            }
        }

        private async Task OrderBook()
        {
            var result = await BookService.OrderBookAsync(_selectedBook.Id);

            if (result.IsSuccess)
            {
                _snackbar.Add("Book ordered successfully", Severity.Success);
                CloseDialog();
                await ReloadTable();
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                _snackbar.Add(result.ErrorMessage, Severity.Error);
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

            var result = await BookService.AddBookAsync(_newBook);

            if (result.IsSuccess)
            {
                _snackbar.Add("Book added successfully.", Severity.Success);
                await ReloadTable();
                _showAddBookDialog = false;
            }
            else
            {
                _snackbar.Add(result.ErrorMessage, Severity.Error);
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

            var result = await BookService.UpdateBookAsync(_bookToEdit);

            if (result.IsSuccess)
            {
                _snackbar.Add("Book updated successfully.", Severity.Success);
                await ReloadTable();
                _showEditBookDialog = false;
            }
            else
            {
                _snackbar.Add(result.ErrorMessage, Severity.Error);
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
            var result = await BookService.DeleteBookAsync(_bookToDelete.Id);

            if (result.IsSuccess)
            {
                _snackbar.Add("Book deleted successfully.", Severity.Success);
                await ReloadTable();
            }
            else
            {
                _snackbar.Add(result.ErrorMessage, Severity.Error);
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
                _isAdmin = await AuthStateService.IsAdmin();
                await ReloadTable();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task ReloadTable()
        {
            if (_table != null)
            {
                await _table.ReloadServerData();
            }
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

        private async Task OnSearchChanged(string search)
        {
            _searchString = search;
            _currentOffset = 0;
            _currentPage = 1;
            await ReloadTable();
        }
    }
}