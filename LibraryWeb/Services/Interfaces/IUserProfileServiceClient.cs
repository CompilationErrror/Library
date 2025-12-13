using DataModelLibrary.Models;

namespace LibraryWeb.Services.Interfaces
{
    public interface IUserProfileServiceClient
    {
        Task<ApiResponse<User>> GetUserByIdAsync(Guid id);
        Task<ApiResponse<User>> GetCurrentUserAsync();
        Task<ApiResponse<UserStats>> GetUserStatsAsync();
        Task<ApiResponse<bool>> UpdateUserAsync(UserUpdateModel updateModel);
    }
}