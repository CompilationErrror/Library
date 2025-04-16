using DataModelLibrary.Models;
using LibraryApi.Data;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<User>> AddUserAsync(User customer)
        {
            await _context.Users.AddAsync(customer);
            await _context.SaveChangesAsync();
            return await _context.Users.ToListAsync();
        }

        public async Task<List<User>> DeleteUserAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.UserTokens
                    .Where(t => t.UserId == id)
                    .ExecuteDeleteAsync();

                var user = await _context.Users.FirstAsync(c => c.Id == id);
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await _context.Users.ToListAsync();
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to delete user", ex);
            }
        }
    }
}
