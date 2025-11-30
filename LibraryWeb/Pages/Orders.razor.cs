using DataModelLibrary.Models;
using LibraryWeb.Services;
using LibraryWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace LibraryWeb.Pages
{
    public partial class Orders
    {
        [Inject]
        private AuthenticationStateService AuthService { get; set; } = default!;
        [Inject]
        private IOrderServiceClient OrderService { get; set; } = default!;

        [Inject]
        private ISnackbar _snackbar { get; set; }

        private bool _isPageLoading = true;
        private bool _isDataLoading = true;
        private List<OrderedBook> _orderedBooks = new();
        private HashSet<OrderedBook> _selectedBooks = new();
        private bool _processingReturns = false;

        private bool _isAuthorized = false;
        protected override async Task OnInitializedAsync()
        {
            _isAuthorized = await AuthService.IsAuthenticated();
            _isPageLoading = false;

            if (_isAuthorized)
            {
                await LoadOrderedBooks();
            }
        }

        private async Task LoadOrderedBooks()
        {
            var result = await OrderService.GetOrderedBooksAsync();

            if (result.IsSuccess)
            {
                _orderedBooks = result.Data ?? new List<OrderedBook>();
            }
            else
            {
                _snackbar.Add(result.ErrorMessage, Severity.Error);
                _orderedBooks = new List<OrderedBook>();
                _isDataLoading = false;
                return;
            }
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
            var response = await OrderService.ReturnBooksAsync(bookIds);

            if (response.IsSuccess)
            {
                _snackbar.Add($"Books were successfully returned", Severity.Success);
                return;
            }
            else
            {
                _snackbar.Add($"Error returning book(s)': {response.ErrorMessage}", Severity.Error);
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