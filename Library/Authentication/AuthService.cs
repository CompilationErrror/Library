using DataModelLibrary.AuthRequestModels;
using DataModelLibrary.Models;
using LibraryApi.Authentication.TokenStore;
using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryApi.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly LibraryContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRedisTokenStore _tokenStore;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationMinutes;

        public AuthService(LibraryContext context, IConfiguration configuration, IRedisTokenStore tokenStore)
        {
            _context = context;
            _configuration = configuration;
            _tokenStore = tokenStore;

            _accessTokenExpirationMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes");
            _refreshTokenExpirationMinutes = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationMinutes");
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                var principal = GetPrincipalFromToken(token, validateLifetime: true);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userId, out var userGuid))
                    return false;

                var user = await _context.Users.FindAsync(userGuid);
                return user != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IssuedTokens> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(c => c.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenExpirationMinutes);

            await _tokenStore.StoreAsync(user.Id, refreshToken, expiresAt);

            return new IssuedTokens(accessToken, refreshToken, expiresAt);
        }

        public async Task<IssuedTokens> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(c => c.Username == request.Username))
            {
                throw new Exception("Username already exists");
            }

            if (await _context.Users.AnyAsync(c => c.Email == request.Email))
            {
                throw new Exception("Email already exists");
            }

            var user = new User
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = HashPassword(request.Password)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenExpirationMinutes);

            await _tokenStore.StoreAsync(user.Id, refreshToken, expiresAt);

            return new IssuedTokens(accessToken, refreshToken, expiresAt);
        }

        public async Task LogoutAsync(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Access token is required");
            }

            var token = accessToken.Replace("Bearer ", "");

            try
            {
                var principal = GetPrincipalFromToken(token, validateLifetime: true);
                var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdString, out var userId))
                {
                    throw new Exception("Invalid token");
                }

                var stored = await _tokenStore.GetAsync(refreshToken);
                if (stored == null || stored.Value.UserId != userId)
                {
                    throw new Exception("Refresh token does not belong to user");
                }

                await _tokenStore.RevokeAsync(refreshToken);
            }
            catch
            {
                throw new Exception("Failed to logout user");
            }
        }

        public async Task<IssuedTokens> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Refresh token is required");
            }

            var stored = await _tokenStore.GetAsync(refreshToken);
            if (stored == null)
            {
                throw new Exception("Invalid refresh token");
            }

            if (stored.Value.ExpiresAt < DateTime.UtcNow)
            {
                await _tokenStore.RevokeAsync(refreshToken);
                throw new Exception("Refresh token expired");
            }

            var user = await _context.Users.FindAsync(stored.Value.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            await _tokenStore.RevokeAsync(refreshToken);

            var newRefreshToken = GenerateRefreshToken();
            var newExpiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenExpirationMinutes);

            await _tokenStore.StoreAsync(user.Id, newRefreshToken, newExpiresAt);

            var newAccessToken = GenerateJwtToken(user);

            return new IssuedTokens(newAccessToken, newRefreshToken, newExpiresAt);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]
                    ?? throw new InvalidOperationException("JWT Key not configured"))),

                ValidateLifetime = validateLifetime,

                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}