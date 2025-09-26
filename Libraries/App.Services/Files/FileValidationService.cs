using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Files
{
    public class FileValidationService : IFileValidationService
    {
        private long _maxSizeBytes = 10 * 1024 * 1024; // 10 MB default
        private string[] _allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };

        public void Configure(long maxSizeBytes, string[] allowedExtensions)
        {
            if (maxSizeBytes > 0) _maxSizeBytes = maxSizeBytes;
            if (allowedExtensions != null && allowedExtensions.Length > 0)
                _allowedExtensions = allowedExtensions.Select(x => x.ToLowerInvariant()).ToArray();
        }

        public Task<bool> ValidateAsync(string fileName, long contentLengthBytes)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return Task.FromResult(false);
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext)) return Task.FromResult(false);
            if (!_allowedExtensions.Contains(ext)) return Task.FromResult(false);
            if (contentLengthBytes <= 0 || contentLengthBytes > _maxSizeBytes) return Task.FromResult(false);
            return Task.FromResult(true);
        }
    }
}
