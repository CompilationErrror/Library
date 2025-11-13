using DataModelLibrary.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace LibraryWeb.Pages
{
    public partial class Orders
    {
        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private ISnackbar _snackbar { get; set; }

        [Inject]
        private AuthenticationStateService AuthStateService { get; set; } = default!;

        private bool _isPageLoading = true;
        private bool _isDataLoading = true;
        private List<OrderedBook> _orderedBooks = new();
        private HashSet<OrderedBook> _selectedBooks = new();
        private bool _processingReturns = false;

        private bool _isAuthorized = false;
        protected override async Task OnInitializedAsync()
        {
            _isAuthorized = await AuthStateService.IsAuthenticated();
            _isPageLoading = false;

            if (_isAuthorized)
            {
                await LoadOrderedBooks();
            }
        }

        private async Task LoadOrderedBooks()
        {
            _orderedBooks = await HttpClient.GetFromJsonAsync<List<OrderedBook>>($"GetOrderedBooksAsync");
            _selectedBooks.Clear();

            _isDataLoading = false;
        }

        private Color GetStatusColor(OrderedBook orderedBook)
        {
            if (orderedBook.OrderDate.AddDays(14) < DateTime.Now)
                return Color.Error;

            return Color.Success;
        }

        private string GetStatusText(OrderedBook orderedBook)
        {
            if (orderedBook.OrderDate.AddDays(14) < DateTime.Now)
                return "Overdue";

            return "Borrowed";
        }

        private async Task ReturnBooks(List<int> bookIds)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "ReturnBook")
            {
                Content = JsonContent.Create(bookIds)
            };

            var response = await HttpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _snackbar.Add($"Books were successfully returned", Severity.Success);
                return;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Error returning book(s)': {errorMessage}", Severity.Error);
            }
        }

        private async Task ReturnSelectedBooks()
        {
            if (_selectedBooks.Count == 0)
            {
                _snackbar.Add("No books selected", Severity.Warning);
                return;
            }

            _processingReturns = true;
            try
            {
                await ReturnBooks(_selectedBooks.Select(sb => sb.BookId).ToList());

                await InvokeAsync(StateHasChanged);
                await LoadOrderedBooks();
            }
            finally
            {
                _processingReturns = false;
            }
        }

        private void OnSelectedBooksChanged(HashSet<OrderedBook> selectedBooks)
        {
            _selectedBooks = selectedBooks;
        }
    }
}