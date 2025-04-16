using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;

namespace LibraryApi.Storage
{
    public class BlobService: IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly Dictionary<string, string> _contentTypes;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = "bookcovers";
            _contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" }
            };

            CreateContainerIfNotExists().GetAwaiter().GetResult();
        }

        private async Task CreateContainerIfNotExists()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(extension) || !_contentTypes.ContainsKey(extension))
            {
                throw new ArgumentException($"Unsupported file type: {extension}. Supported types are: {string.Join(", ", _contentTypes.Keys)}");
            }

            return _contentTypes[extension];
        }

        private string SanitizeFileName(string fileName)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegEx = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(fileName, invalidRegEx, "_");
        }

        public async Task<string> UploadImageAsync(string imageName, Stream content)
        {
            string sanitizedName = SanitizeFileName(imageName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(sanitizedName);

            string contentType = GetContentType(sanitizedName);

            await blobClient.UploadAsync(content, new BlobHttpHeaders
            {
                ContentType = contentType,
            });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string imageName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(imageName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
