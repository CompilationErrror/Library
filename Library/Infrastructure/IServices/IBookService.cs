using DataModelLibrary.Models;
using DataModelLibrary.QueryParameters;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetBooksAsync(BookQueryParameters parameters);
        Task<Book> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);
        Task<int>GetTotalCountAsync();
    }
}
