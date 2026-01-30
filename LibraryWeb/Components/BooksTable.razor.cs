using DataModelLibrary.FilterModels;
using DataModelLibrary.Models;
using LibraryWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LibraryWeb.Components
{
    public partial class BooksTable
    {
        [Inject] private IBookServiceClient BookService { get; set; } = default!;

        [Parameter, EditorRequired] 
        public Func<TableState, CancellationToken, Task<TableData<Book>>> ServerData { get; set; } = default!;
        [Parameter] public bool IsAdmin { get; set; }
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public EventCallback<string> OnSearchChanged { get; set; }
        [Parameter] public EventCallback<TableRowClickEventArgs<Book>> OnRowClick { get; set; }
        [Parameter] public EventCallback<Book> OnDeleteBook { get; set; }
        [Parameter] public EventCallback<Book> OnEditBook { get; set; }
        [Parameter] public EventCallback<Book> OnOrderBook { get; set; }
        [Parameter] public EventCallback<BookFilter> OnFilterChanged { get; set; }
        [Parameter] public BookFilter Filter { get; set; } = new();
        [Parameter] public List<Genre> AvailableGenres { get; set; } = new();

        private int GenreId { get; set; }
        private IEnumerable<int> selectedGenres { get; set; } = new List<int>();

        private MudTable<Book>? _table;

        private string _searchString = "";

        private readonly int[] PageSizeOptions = { 5, 10, 20, 50 };

        private bool _showFilterDialog;

        private BookFilter _localFilter = new();

        private async Task DeleteBook(Book book)
            => await OnDeleteBook.InvokeAsync(book);

        private async Task EditBook(Book book)
            => await OnEditBook.InvokeAsync(book);

        private async Task OrderBook(Book book)
            => await OnOrderBook.InvokeAsync(book);

        private async Task OnSearchInternal(string value)
        {
            _searchString = value;

            if (OnSearchChanged.HasDelegate)
                await OnSearchChanged.InvokeAsync(value);
        }

        private void OpenFilterDialog()
        {
            _localFilter = new BookFilter
            {
                Author = Filter.Author,
                YearFrom = Filter.YearFrom,
                YearTo = Filter.YearTo,
                PriceFrom = Filter.PriceFrom,
                PriceTo = Filter.PriceTo,
                AvailableOnly = Filter.AvailableOnly,
                GenreIds = Filter.GenreIds != null ? new List<int>(Filter.GenreIds) : null
            };

            _showFilterDialog = true;
        }

        private async Task ApplyFilters()
        {
            _showFilterDialog = false;

            await OnFilterChanged.InvokeAsync(_localFilter);

            await ReloadTableData();
        }

        private async Task ClearFilters()
        {
            _localFilter = new BookFilter();
            _showFilterDialog = false;

            await OnFilterChanged.InvokeAsync(_localFilter);

            await ReloadTableData();
        }

        public async Task ReloadTableData()
        {
            if (_table != null)
            {
                await _table.ReloadServerData();
            }
        }
    }

}
