using Financist.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace Financist.Infrastructure.Services;

public sealed class LocalDocumentStorageService : IDocumentStorageService
{
    private readonly string _rootPath;

    public LocalDocumentStorageService(IOptions<StorageOptions> options)
    {
        var configuredPath = options.Value.DocumentsPath;
        _rootPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
    }

    public async Task<StoredDocument> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);

        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var now = DateTime.UtcNow;
        var relativeFolder = Path.Combine(now.ToString("yyyy"), now.ToString("MM"));
        var absoluteFolder = Path.Combine(_rootPath, relativeFolder);
        Directory.CreateDirectory(absoluteFolder);

        var absoluteFilePath = Path.Combine(absoluteFolder, storedFileName);
        await using var fileStream = File.Create(absoluteFilePath);
        await content.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);

        var sizeBytes = fileStream.Length;
        var relativePath = Path.Combine(relativeFolder, storedFileName).Replace("\\", "/", StringComparison.Ordinal);

        return new StoredDocument(storedFileName, relativePath, sizeBytes);
    }
}
