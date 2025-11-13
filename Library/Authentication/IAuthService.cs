using DataModelLibrary.AuthRequestModels;

namespace LibraryApi.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(string accessToken, string refreshToken);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ValidateTokenAsync(string token);
    }
}