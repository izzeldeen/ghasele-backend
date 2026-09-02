using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ghasele.Application.Interfaces;

namespace Ghasele.API.Services
{
    /// <summary>
    /// Saves uploads under <c>wwwroot/uploads/{folder}</c> so they are served
    /// straight back by <c>UseStaticFiles</c>. Fine for a single instance; swap
    /// for blob storage before scaling out.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _webRootPath;
        private const string UploadsRoot = "uploads";

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            // WebRootPath is null until wwwroot exists; fall back to <content>/wwwroot.
            _webRootPath = env.WebRootPath
                ?? Path.Combine(env.ContentRootPath, "wwwroot");
        }

        public async Task<string> SaveAsync(Stream content, string originalFileName, string folder, CancellationToken cancellationToken = default)
        {
            var safeFolder = string.IsNullOrWhiteSpace(folder) ? "misc" : folder.Trim('/', '\\');
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension) || extension.Length > 10)
            {
                extension = ".bin";
            }

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var targetDir = Path.Combine(_webRootPath, UploadsRoot, safeFolder);
            Directory.CreateDirectory(targetDir);

            var fullPath = Path.Combine(targetDir, fileName);
            await using (var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(file, cancellationToken);
            }

            return $"/{UploadsRoot}/{safeFolder}/{fileName}";
        }
    }
}
