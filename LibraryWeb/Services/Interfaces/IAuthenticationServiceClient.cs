using DataModelLibrary.AuthRequestModels;

namespace LibraryWeb.Services.Interfaces
{
    public interface IAuthenticationServiceClient
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<bool> IsAuthenticatedAsync();
        Task LogoutAsync();
    }
}