using DataModelLibrary.Models;
using LibraryWeb.Services.Base;
using LibraryWeb.Services.Interfaces;

namespace LibraryWeb.Services
{
    public class UserProfileServiceClient : ApiBaseClient, IUserProfileServiceClient
    {
        public UserProfileServiceClient(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<User>> GetUserByIdAsync(Guid id)
            => GetAsync<User>($"api/User/{id}");

        public Task<ApiResponse<UserStats>> GetUserStatsAsync()
            => GetAsync<UserStats>("api/User/stats");

        public Task<ApiResponse<bool>> UpdateUserAsync(UserUpdateModel updateModel)
            => PutJsonAsync<bool>("api/User", updateModel);
    }
}