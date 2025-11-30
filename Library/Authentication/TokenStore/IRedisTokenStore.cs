namespace LibraryApi.Authentication.TokenStore
{
    public interface IRedisTokenStore
    {
        Task StoreAsync(Guid userId, string refreshToken, DateTime expiresAt);
        Task<(Guid UserId, DateTime ExpiresAt)?> GetAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
    }
}
