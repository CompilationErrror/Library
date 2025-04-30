using DataModelLibrary.Models;
using LibraryApi.Data;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Services
{
    public class OrderService: IOrderService
    {
        private readonly LibraryContext _context;
        private const int MaxBooksPerUser = 5;

        public OrderService(LibraryContext context)
        {
            _context = context;
        }

        public async Task PlaceBookOrder(int bookId, Guid userId)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId) ?? throw new ArgumentException("Book not found");

            var customerOrders = await _context.OrderedBooks
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (customerOrders.Count >= MaxBooksPerUser)
            {
                throw new InvalidOperationException($"User has reached the maximum limit of {MaxBooksPerUser} books");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(c => c.Id == userId) ?? throw new ArgumentException("User not found");

            var orderedBook = new OrderedBook
            {
                BookId = bookId,
                UserId = userId,
                Book = book,
                User = user
            };

            book.IsAvailable = false;

            await _context.OrderedBooks.AddAsync(orderedBook);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderedBook>> GetOrderedBooksByUserId(Guid userId) 
        {
            var orderedBooks = await _context.OrderedBooks
                .Include(ob => ob.Book)
                .Where(ob => ob.UserId == userId)
                .ToListAsync();
            return orderedBooks;
        }
        public async Task DeleteOrderedBooksByIdAsync(List<int> bookIds)
        {
            var orderedBooksToDelete = await _context.OrderedBooks
            .Include(ob => ob.Book)
            .Where(ob => bookIds.Contains(ob.BookId))
            .ToListAsync();

            var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == orderedBooksToDelete.First().UserId);
            user.TotalBooksReturned++;

            orderedBooksToDelete.ForEach(ob => ob.Book.IsAvailable = true);

            _context.OrderedBooks.RemoveRange(orderedBooksToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
