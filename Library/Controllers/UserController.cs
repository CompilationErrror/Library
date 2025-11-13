using DataModelLibrary.Models;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _customerService;
        public UserController( IUserService customerService)
        {
            _customerService = customerService; 
        }

        [HttpGet("/GetUsers")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _customerService.GetUsersAsync());
        }

        [HttpGet("/GetUserById")]
        public async Task<ActionResult<User>> GetUserById()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user token");
            }
            var user = await _customerService.GetUserByIdAsync(userId);
            return Ok(user);
        }

        [HttpGet("/GetUserStats")]
        public async Task<ActionResult<UserStats>> GetUserStats() 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user token");
            }
            var userStats = await _customerService.GetUserStatsAsync(userId);
            return Ok(userStats);
        }

        [HttpPut("/UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserUpdateModel userToUpdate) 
        {
            await _customerService.UpdateUserAsync(userToUpdate);
            return NoContent();
        }

        [HttpPost("/AddUser")]
        public async Task<IActionResult> AddUser(User customer)
        {
            await _customerService.AddUserAsync(customer);
            return Created();
        }

        [HttpDelete("/DeleteUser")]
        public async Task<IActionResult> DeleteUser([Required] Guid id)
        {
            await _customerService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
