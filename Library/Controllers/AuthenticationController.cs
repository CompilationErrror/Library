using DataModelLibrary.AuthRequestModels;
using LibraryApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var issued = await _authService.RegisterAsync(request);

            if (!string.IsNullOrEmpty(issued.RefreshToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = issued.ExpiresAt,
                    Path = "/"
                };
                Response.Cookies.Append("refreshToken", issued.RefreshToken, cookieOptions);
            }

            return Ok(new AuthResponse
            {
                AccessToken = issued.AccessToken,
                ExpiresAt = issued.ExpiresAt
            });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var issued = await _authService.LoginAsync(request);

            if (!string.IsNullOrEmpty(issued.RefreshToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = issued.ExpiresAt,
                    Path = "/"
                };
                Response.Cookies.Append("refreshToken", issued.RefreshToken, cookieOptions);
            }

            return Ok(new AuthResponse
            {
                AccessToken = issued.AccessToken,
                ExpiresAt = issued.ExpiresAt
            });
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<AuthResponse>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is required");
            }

            var issued = await _authService.RefreshTokenAsync(refreshToken);

            if (!string.IsNullOrEmpty(issued.RefreshToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = issued.ExpiresAt,
                    Path = "/"
                };
                Response.Cookies.Append("refreshToken", issued.RefreshToken, cookieOptions);
            }

            return Ok(new AuthResponse
            {
                AccessToken = issued.AccessToken,
                ExpiresAt = issued.ExpiresAt
            });
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<ActionResult> Logout()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized();

            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Refresh token missing");

            await _authService.LogoutAsync(authHeader, refreshToken);

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return NoContent();
        }

        [HttpPost("ValidateToken")]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(isValid);
        }
    }
}