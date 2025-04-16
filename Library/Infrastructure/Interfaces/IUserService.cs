using DataModelLibrary.Models;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<List<User>> AddUserAsync(User customer);
        Task<List<User>> DeleteUserAsync(Guid id);
    }
}
