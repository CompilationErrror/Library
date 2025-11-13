using LibraryApi.Authentication.TokenStore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LibraryApi.Authentication
{
    public class RedisTokenStore : IRedisTokenStore
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web);

        public RedisTokenStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task StoreAsync(Guid userId, string refreshToken, DateTime expiresAt)
        {
            var data = new TokenData
            {
                UserId = userId,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt
            };

            await _cache.SetStringAsync(
                $"refresh:{refreshToken}",
                JsonSerializer.Serialize(data, _opts),
                options);
        }

        public async Task<(Guid UserId, DateTime ExpiresAt, bool IsRevoked)?> GetAsync(string refreshToken)
        {
            var json = await _cache.GetStringAsync($"refresh:{refreshToken}");
            if (json == null) return null;

            var data = JsonSerializer.Deserialize<TokenData>(json, _opts);
            if (data == null) return null;

            return (data.UserId, data.ExpiresAt, data.IsRevoked);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            await _cache.RemoveAsync($"refresh:{refreshToken}");
        }

        private class TokenData
        {
            public Guid UserId { get; set; }
            public DateTime ExpiresAt { get; set; }
            public bool IsRevoked { get; set; }
        }
    }
}