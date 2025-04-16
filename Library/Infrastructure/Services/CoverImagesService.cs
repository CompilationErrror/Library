using DataModelLibrary.Models;
using LibraryApi.Data;
using LibraryApi.Infrastructure.Interfaces;
using LibraryApi.Storage;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Services
{
    public class CoverImagesService: ICoverImagesService
    {
        private readonly LibraryContext _context;
        private readonly IBlobService _blobService;
        public CoverImagesService(LibraryContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<string> GetCoverAsync(int bookId)
        {
            var coverImage = await _context.CoverImages
                .FirstOrDefaultAsync(c => c.BookId == bookId)
                ?? throw new FileNotFoundException($"No cover found for book {bookId}");

            return coverImage.CoverImageUrl;
        }
        public async Task<string> AddCoverAsync(int bookId, Stream imageStream, string fileName)
        {
            string uniqueFileName = $"{bookId}_{DateTime.UtcNow.Ticks}_{fileName}";

            string imageUrl = await _blobService.UploadImageAsync(uniqueFileName, imageStream);

            var cover = new CoverImages
            {
                BookId = bookId,
                CoverImageUrl = imageUrl
            };

            await _context.CoverImages.AddAsync(cover);
            await _context.SaveChangesAsync();

            return imageUrl;
        }

        public async Task DeleteCoverAsync(int bookId)
        {
            var coverToDelete = await _context.CoverImages.FirstAsync(c => c.BookId == bookId);

            if (!string.IsNullOrEmpty(coverToDelete.CoverImageUrl))
            {
                string fileName = coverToDelete.CoverImageUrl.Split('/').Last();
                await _blobService.DeleteImageAsync(fileName);
            }

            _context.Remove(coverToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
