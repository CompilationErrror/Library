using DataModelLibrary.AuthModels;
using DataModelLibrary.AuthRequestModels;

namespace LibraryApi.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<UserToken> GetUserTokenInstanceAsync(string accessToken);
        Task LogoutAsync(string token);
        Task<bool> ValidateTokenAsync(string token);
    }
}