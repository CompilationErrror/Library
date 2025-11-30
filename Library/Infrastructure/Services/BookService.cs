using DataModelLibrary.Models;
using LibraryApi.Data;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Services
{
    public class BookService : IBookService
    {
        private readonly LibraryContext _context;

        public BookService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            var book =  await _context.Books.Include(x => x.Cover)
                .FirstOrDefaultAsync(b => b.Id == id);
            return book;
        }

        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var bookToDelete = await _context.Books.Include(x => x.Cover)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bookToDelete == null)
                throw new KeyNotFoundException($"Error while deleting a book");

            _context.Books.Remove(bookToDelete);
            await _context.SaveChangesAsync();
        }
    }
}