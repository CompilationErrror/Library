using DataModelLibrary.AuthModels;
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
            var token = await _authService.RegisterAsync(request);
            return Ok(token);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var token = await _authService.LoginAsync(request);
            return Ok(token);
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<ActionResult> Logout()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized();

            await _authService.LogoutAsync(authHeader);
            return Ok();
        }

        [HttpPost("ValidateToken")]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(isValid);
        }

        [HttpPost("GetTokenInstance")]
        public async Task<ActionResult<UserToken>> GetTokenInstance(string accessToken)
        {
            var response = await _authService.GetUserTokenInstanceAsync(accessToken);
            return Ok(response);
        }
    }
}