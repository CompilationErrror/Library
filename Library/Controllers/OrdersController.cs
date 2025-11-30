using DataModelLibrary.Models;
using LibraryApi.Extensions;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderedBook>>> GetOrderedBooks()
        {
            var userId = User.GetUserId();

            var orderedBooks = await _orderService.GetOrderedBooksByUserIdAsync(userId);
            return Ok(orderedBooks);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> OrderBook([FromRoute] int id)
        {
            var userId = User.GetUserId();

            await _orderService.PlaceBookOrderAsync(id, userId);
            return NoContent();                
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrderedBook([FromBody] List<int> bookIds)
        {
            await _orderService.DeleteOrderedBooksByIdAsync(bookIds);
            return NoContent();
        }
    }
}
