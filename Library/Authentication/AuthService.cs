using DataModelLibrary.AuthModels;
using DataModelLibrary.AuthRequestModels;
using DataModelLibrary.Models;
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

        public AuthService(LibraryContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;
            try
            {
                var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);

                if (userToken == null)
                    return false;

                if (userToken.ExpiresAt <= DateTime.UtcNow)
                {
                    userToken.IsRevoked = true;
                    await _context.SaveChangesAsync();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Problem occured while getting token from db",ex);
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

            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = accessToken,
                RefreshToken = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = userToken.ExpiresAt
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(c => c.Username == request.Username))
                throw new Exception("Username already exists");

            if (await _context.Users.AnyAsync(c => c.Email == request.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = HashPassword(request.Password)
            };

            await _context.Users.AddAsync(user);

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = accessToken,
                RefreshToken = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = userToken.ExpiresAt
            };
        }

        public async Task LogoutAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required");

            var token = accessToken.Replace("Bearer ", "");

            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);

            if (userToken == null)
                throw new Exception("Invalid or expired token");

            userToken.IsRevoked = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to logout user", ex);
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            var username = principal.Identity?.Name;

            var user = await _context.Users
                .Include(u => u.Tokens)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                throw new Exception("User not found");

            var existingToken = await _context.UserTokens
                .FirstOrDefaultAsync(t =>
                    t.UserId == user.Id &&
                    t.RefreshToken == request.RefreshToken &&
                    !t.IsRevoked);

            if (existingToken == null)
                throw new Exception("Invalid refresh token");

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            existingToken.IsRevoked = true;

            var newUserToken = new UserToken
            {
                UserId = user.Id,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.UserTokens.AddAsync(newUserToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = newUserToken.ExpiresAt
            };
        }

        public async Task<UserToken> GetUserTokenInstanceAsync(string accessToken) 
        {
            return await _context.UserTokens.FirstOrDefaultAsync(ut => ut.Token == accessToken);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
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
                ValidAudience = _configuration["JwtSettings:Audience"]
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
                expires: DateTime.Now.AddDays(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
