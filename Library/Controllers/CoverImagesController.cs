using DataModelLibrary.Models;
using LibraryApi.Infrastructure.Interfaces;
using LibraryApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoverImagesController : ControllerBase
    {
        private readonly ICoverImagesService _coverImagesService;
        public CoverImagesController(ICoverImagesService coverImagesService)
        {
            _coverImagesService = coverImagesService;
        }

        [HttpGet("/GetCover")]
        public async Task<IActionResult> GetCover([Required]int id = 0)
        {
            var coverUri = await _coverImagesService.GetCoverAsync(id);
            return Ok(coverUri);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/AddCover")]
        public async Task<IActionResult> AddCover(IFormFile coverImg, int bookId)
        {
            if (coverImg == null || coverImg.Length == 0)
            {
                return BadRequest("Cover image file is required");
            }

            using var stream = coverImg.OpenReadStream();
            var imageUrl = await _coverImagesService.AddCoverAsync(bookId, stream, coverImg.FileName);
            return Ok(imageUrl);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/DeleteCover")]
        public async Task<IActionResult> DeleteCover([Required] int bookId)
        {
            await _coverImagesService.DeleteCoverAsync(bookId);
            return NoContent();
        }
    }
}
