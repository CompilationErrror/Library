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
    [Authorize]
    public class CoverImagesController : ControllerBase
    {
        private readonly ICoverImagesService _coverImagesService;
        public CoverImagesController(ICoverImagesService coverImagesService)
        {
            _coverImagesService = coverImagesService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCover([FromRoute]int id = 0)
        {
            var cover = await _coverImagesService.GetCoverAsync(id);

            if (cover is null)
            {
                return NoContent();
            }
            return Ok(cover.CoverImageUrl);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddCover(IFormFile coverImg,[FromRoute] int bookId)
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
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteCover([FromRoute] int bookId)
        {
            await _coverImagesService.DeleteCoverAsync(bookId);
            return NoContent();
        }
    }
}
