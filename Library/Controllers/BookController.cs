using DataModelLibrary.Models;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet("/GetBooks")]
        public async Task<ActionResult<List<Book>>> GetBooks(int id = 0)
        {
            var books = await _bookService.GetBooksAsync(id);
            return Ok(books);
        }

        [HttpPost("/AddBook")]
        public async Task<IActionResult> AddBook(Book book)
        {
            await _bookService.AddBookAsync(book);
            return NoContent();
        }

        [HttpPut("/ChangeBook")]
        public async Task<IActionResult> ChangeBook(Book book)
        {
            await _bookService.UpdateBookAsync(book);
            return NoContent();
        }

        [HttpDelete("/DeleteBook")]
        public async Task<IActionResult> DeleteBook([Required] int id)
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }
    }
}