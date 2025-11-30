using DataModelLibrary.AuthRequestModels;

namespace LibraryApi.Authentication
{
    // IssuedTokens contains the raw refresh token so controllers can place it in a cookie.
    public record IssuedTokens(string AccessToken, string RefreshToken, DateTime ExpiresAt);

    public interface IAuthService
    {
        Task<IssuedTokens> RegisterAsync(RegisterRequest request);
        Task<IssuedTokens> LoginAsync(LoginRequest request);
        Task LogoutAsync(string accessToken, string refreshToken);
        Task<IssuedTokens> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
    }
}