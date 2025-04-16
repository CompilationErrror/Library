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

        [HttpGet("/GetCustomers")]
        public async Task<List<User>> GetCustomers()
        {
            return await _customerService.GetUsersAsync();
        }

        [HttpPost("/AddCustomer")]
        public async Task<List<User>> AddCustomer(User customer)
        {
            return await _customerService.AddUserAsync(customer);
        }

        [HttpDelete("/DeleteCustomer")]
        public async Task<List<User>> DeleteCustomer([Required] Guid id)
        {
            return await _customerService.DeleteUserAsync(id);
        }
    }
}
