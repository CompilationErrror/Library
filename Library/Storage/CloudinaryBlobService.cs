using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace LibraryApi.Storage
{
    public class CloudinaryBlobService : IBlobService
    {
        private readonly Cloudinary _cloudinary;
        private readonly Dictionary<string, string> _contentTypes;

        public CloudinaryBlobService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            _cloudinary = new Cloudinary(new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret));

            _contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" }
            };
        }

        private void ValidateContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(extension) || !_contentTypes.ContainsKey(extension))
            {
                throw new ArgumentException($"Unsupported file type: {extension}. Supported types are: {string.Join(", ", _contentTypes.Keys)}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegEx = string.Format(@"[{0}]+", invalidChars);

            return Regex.Replace(nameWithoutExtension, invalidRegEx, "_");
        }

        public async Task<string> UploadImageAsync(string imageName, Stream content)
        {
            ValidateContentType(imageName);

            string sanitizedName = SanitizeFileName(imageName);

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(imageName, content),
                PublicId = $"bookcovers/{sanitizedName}",
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Failed to upload image: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task DeleteImageAsync(string imageName)
        {
            string publicId;
            if (imageName.StartsWith("http"))
            {
                var uri = new Uri(imageName);
                var pathSegments = uri.AbsolutePath.Split('/');
                var versionIndex = Array.FindIndex(pathSegments, s => s.StartsWith("v"));
                if (versionIndex > 0 && versionIndex < pathSegments.Length - 1)
                {
                    publicId = string.Join("/", pathSegments.Skip(versionIndex + 1));
                    publicId = Path.GetFileNameWithoutExtension(publicId); 
                }
                else
                {
                    publicId = Path.GetFileNameWithoutExtension(pathSegments.Last());
                }
            }
            else
            {
                publicId = $"bookcovers/{SanitizeFileName(imageName)}";
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}