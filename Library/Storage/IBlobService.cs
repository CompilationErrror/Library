namespace LibraryApi.Storage
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(string imageName, Stream content);
        Task DeleteImageAsync(string imageName);
    }
}
