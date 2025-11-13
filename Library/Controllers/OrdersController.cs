using DataModelLibrary.Models;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("/OrderBook")]
        public async Task<IActionResult> OrderBook([Required] int bookId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user token");
            }
            await _orderService.PlaceBookOrder(bookId, userId);
            return NoContent();                
        }

        [HttpGet("/GetOrderedBooksAsync")]
        public async Task<ActionResult<List<OrderedBook>>> GetOrderedBooksAsync() 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user token");
            }
            var orderedBooks = await _orderService.GetOrderedBooksByUserId(userId);
            return Ok(orderedBooks);
        }

        [HttpDelete("/ReturnBook")]
        public async Task<IActionResult> DeleteOrderedBook([FromBody] List<int> bookIds)
        {
            await _orderService.DeleteOrderedBooksByIdAsync(bookIds);
            return NoContent();
        }
    }
}
