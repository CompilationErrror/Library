using DataModelLibrary.Models;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        public async Task<List<User>> GetUser()
        {
            return await _customerService.GetUsersAsync();
        }

        [HttpGet("/GetUserById")]
        public async Task<User> GetUserById(Guid userId)
        {
            return await _customerService.GetUserByIdAsync(userId);
        }

        [HttpGet("/GetUserStats")]
        public async Task<UserStats> GetUserStats(Guid userId) 
        {
            return await _customerService.GetUserStatsAsync(userId);
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
