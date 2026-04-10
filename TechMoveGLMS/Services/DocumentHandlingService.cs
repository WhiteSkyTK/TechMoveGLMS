using Microsoft.AspNetCore.Http;

namespace TechMoveGLMS.Services
{
    public class DocumentHandlingService
    {
        private readonly string _uploadDirectory = "wwwroot/uploads/contracts";

        public async Task<string> UploadPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            // Strict Validation
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                throw new InvalidOperationException("Only PDF files are allowed.");

            // UUID Naming to prevent overwrites
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadDirectory, uniqueFileName);

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), _uploadDirectory));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/contracts/{uniqueFileName}";
        }
    }
}