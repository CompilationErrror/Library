using DataModelLibrary.Models;

namespace LibraryWeb.Services.Interfaces
{
    public interface IOrderServiceClient
    {
        Task<ApiResponse<List<OrderedBook>>> GetOrderedBooksAsync();
        Task<ApiResponse<bool>> ReturnBooksAsync(List<int> bookIds);
        Task<ApiResponse<bool>> OrderBookAsync(int bookId);
    }
}