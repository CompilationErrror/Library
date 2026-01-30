using DataModelLibrary.Models;
using DataModelLibrary.Pagination;
using DataModelLibrary.QueryParameters;
using LibraryApi.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetBooks([FromQuery] BookQueryParameters parameters)
        {
            var books = await _bookService.GetBooksAsync(parameters);
            var totalCount = await _bookService.GetTotalCountAsync(parameters);

            return Ok(new PagedResult<Book>
            {
                Items = books,
                TotalCount = totalCount
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<Book>>> GetBookById([FromRoute] int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            return Ok(book);
        }

        [HttpGet("genres")]
        public async Task<ActionResult<List<Genre>>> GetGenres()
        {
            var genres = await _bookService.GetGenresAsync();
            return Ok(genres);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(Book book)
        {
            await _bookService.AddBookAsync(book);
            return Created();
        }

        [HttpPut]
        public async Task<IActionResult> ChangeBook(Book book)
        {
            await _bookService.UpdateBookAsync(book);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook([FromRoute] int id)
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }
    }
}