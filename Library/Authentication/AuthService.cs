using Azure.Core;
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
                return false;

            try
            {
                var principal = GetPrincipalFromToken(token);
                if (principal?.Identity?.Name == null)
                    return false;

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userId, out var userGuid))
                {
                    var user = await _context.Users.FindAsync(userGuid);
                    return user != null;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(c => c.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                throw new Exception("Invalid username or password");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenExpirationMinutes);

            await _tokenStore.StoreAsync(user.Id, refreshToken, expiresAt);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
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

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
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
                var principal = GetPrincipalFromToken(token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userId, out var userGuid))
                {
                    throw new Exception("Invalid token");
                }

                await _tokenStore.RevokeAsync(refreshToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to logout user", ex);
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromToken(request.AccessToken);
            var username = principal.Identity?.Name;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var existing = await _tokenStore.GetAsync(request.RefreshToken);

            if (existing == null || existing.Value.IsRevoked || existing.Value.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Invalid refresh token");
            }

            await _tokenStore.RevokeAsync(request.RefreshToken);

            var newRefreshToken = GenerateRefreshToken();
            var newExpiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenExpirationMinutes);

            await _tokenStore.StoreAsync(user.Id, newRefreshToken, newExpiresAt);

            var newAccessToken = GenerateJwtToken(user);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = newExpiresAt
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))),
                ValidateLifetime = false,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
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