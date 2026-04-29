using Financist.Application.Abstractions.Storage;
using Financist.Application.Common.Exceptions;
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

    public Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        var normalizedPath = storagePath.Replace("/", Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        var absoluteRoot = Path.GetFullPath(_rootPath);
        var absoluteFilePath = Path.GetFullPath(Path.Combine(absoluteRoot, normalizedPath));
        var absoluteRootWithSeparator = absoluteRoot.EndsWith(Path.DirectorySeparatorChar)
            ? absoluteRoot
            : $"{absoluteRoot}{Path.DirectorySeparatorChar}";

        if (!absoluteFilePath.StartsWith(absoluteRootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["storagePath"] = ["Invalid document storage path."]
            });
        }

        if (!File.Exists(absoluteFilePath))
        {
            throw new NotFoundException("Stored document was not found.");
        }

        Stream stream = File.OpenRead(absoluteFilePath);
        return Task.FromResult(stream);
    }
}
