using DataModelLibrary.Models;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetBooksAsync(int id = 0);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);
    }
}
