using DataModelLibrary.Models;
using LibraryApi.Data;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LibraryApi.Infrastructure.Services
{
    public class UserService: IUserService
    {

        private readonly LibraryContext _context;

        public UserService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserStats> GetUserStatsAsync(Guid userId)
        {
            var today = DateTime.Now;

            var user = await _context.Users.FindAsync(userId);

            var currentlyBorrowed = await _context.OrderedBooks
                .CountAsync(ob => ob.UserId == userId);

            var overdueBooks = await _context.OrderedBooks
                .CountAsync(ob => ob.UserId == userId && ob.ReturnDate < today);

            var stats = new UserStats
            {
                CurrentlyBorrowed = currentlyBorrowed,
                BooksReturned = user?.TotalBooksReturned ?? 0,
                OverdueBooks = overdueBooks
            };

            return stats;
        }

        public async Task UpdateUserAsync(UserUpdateModel userToUpdate)
        {
            var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userToUpdate.UserId);

            if (user == null)
                throw new Exception("User not found");

            if (userToUpdate.Name != null)
                user.Name = userToUpdate.Name;

            if (userToUpdate.Surname != null)
                user.Surname = userToUpdate.Surname;

            if (userToUpdate.Email != null)
                user.Email = userToUpdate.Email;

            if (!string.IsNullOrEmpty(userToUpdate.CurrentPassword) && !string.IsNullOrEmpty(userToUpdate.NewPassword))
            {
                string currentPasswordHash = HashPassword(userToUpdate.CurrentPassword);

                if (currentPasswordHash != user.PasswordHash)
                    throw new Exception("Current password is incorrect");

                user.PasswordHash = HashPassword(userToUpdate.NewPassword);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FirstAsync(c => c.Id == userId);
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to delete user", ex);
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
