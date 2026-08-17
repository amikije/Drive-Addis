using DriveAddis.Application.Interfaces;

namespace DriveAddis.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken ct)
    {
        var targetDirectory = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(targetDirectory);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(targetDirectory, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, ct);
        }

        return $"/uploads/{folder}/{uniqueFileName}";
    }
}