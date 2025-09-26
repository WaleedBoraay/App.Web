using System.Threading.Tasks;

namespace App.Services.Files
{
    public interface IFileValidationService
    {
        Task<bool> ValidateAsync(string fileName, long contentLengthBytes);
        void Configure(long maxSizeBytes, string[] allowedExtensions);
    }
}
