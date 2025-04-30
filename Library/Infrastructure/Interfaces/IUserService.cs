using DataModelLibrary.Models;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(Guid userId);
        Task AddUserAsync(User customer);
        Task<UserStats> GetUserStatsAsync(Guid userId);
        Task UpdateUserAsync(UserUpdateModel userToUpdate);
        Task DeleteUserAsync(Guid id);
    }
}
