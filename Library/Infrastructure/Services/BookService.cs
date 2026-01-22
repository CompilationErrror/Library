using DataModelLibrary.Models;
using DataModelLibrary.QueryParameters;
using LibraryApi.Data;
using LibraryApi.Extensions.ApiQueryExtensions;
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

        public async Task<List<Book>> GetBooksAsync(BookQueryParameters parameters)
        {
            var query = _context.Books.AsNoTracking();

            query = query.ApplyFiltering(parameters);

            query = query.ApplySorting(parameters.SortBy, parameters.SortDescending);

            return await query
                .Skip(parameters.Offset)
                .Take(parameters.Limit)
                .ToListAsync();
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

        public async Task<int> GetTotalCountAsync(BookQueryParameters parameters)
        {
            var query = _context.Books.AsNoTracking();

            query = query.ApplyFiltering(parameters);

            return await query.CountAsync();
        }
    }
}