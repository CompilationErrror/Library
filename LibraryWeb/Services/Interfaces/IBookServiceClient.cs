using DataModelLibrary.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace LibraryWeb.Services.Interfaces
{
    public interface IBookServiceClient
    {
        Task<ApiResponse<List<Book>>> GetBooksAsync();
        Task<ApiResponse<string>> GetBookCoverAsync(int bookId);
        Task<ApiResponse<string>> UploadBookCoverAsync(int bookId, IBrowserFile file);
        Task<ApiResponse<bool>> DeleteBookCoverAsync(int bookId);
        Task<ApiResponse<bool>> OrderBookAsync(int bookId);
        Task<ApiResponse<Book>> AddBookAsync(Book book);
        Task<ApiResponse<Book>> UpdateBookAsync(Book book);
        Task<ApiResponse<bool>> DeleteBookAsync(int bookId);
    }
}