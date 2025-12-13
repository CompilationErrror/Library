using DataModelLibrary.Models;

namespace LibraryApi.Infrastructure.Interfaces
{
    public interface ICoverImagesService
    {
        Task<CoverImage> GetCoverAsync(int bookId);
        Task<string> AddCoverAsync(int bookId, Stream imageStream, string fileName);
        Task DeleteCoverAsync(int bookId);
    }
}
