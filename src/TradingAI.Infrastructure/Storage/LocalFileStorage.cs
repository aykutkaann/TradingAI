using Microsoft.AspNetCore.Hosting;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace TradingAI.Infrastructure.Storage
{
    public class LocalFileStorage :IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowedExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

        public LocalFileStorage(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> SaveAsync(Stream stream, string fileName, string userId, CancellationToken ct)
        {

            //Check extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ValidationException();
            }

            //Build disk path

            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var relativeFolder = Path.Combine("uploads", "charts", userId.ToString());
            var fullFolder = Path.Combine(root, relativeFolder);

            if (!Directory.Exists(fullFolder))
            {
                Directory.CreateDirectory(fullFolder);
            }

            // 3. Generate Secure Filename
            var newName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(fullFolder, newName);

            // 4. Write to Disk
            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fs, ct);
            }

            // 5. Return Browser URL (Forward Slashes)
            return $"/uploads/charts/{userId}/{newName}";
        }

        public  Task DeleteAsync(string url, CancellationToken ct)
        {

            if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

            var filePath = Path.Combine(root, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

    }
}
