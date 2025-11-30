using DataModelLibrary.Models;
using LibraryApi.Extensions;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _usererService;
        public UserController( IUserService userService)
        {
            _usererService = userService; 
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _usererService.GetUsersAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById([FromRoute]Guid id)
        {
            var user = await _usererService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("/stats")]
        public async Task<ActionResult<UserStats>> GetUserStats() 
        {
            var userId = User.GetUserId();

            var userStats = await _usererService.GetUserStatsAsync(userId);
            return Ok(userStats);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UserUpdateModel userToUpdate) 
        {
            await _usererService.UpdateUserAsync(userToUpdate);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(User customer)
        {
            await _usererService.AddUserAsync(customer);
            return Created();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            await _usererService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
