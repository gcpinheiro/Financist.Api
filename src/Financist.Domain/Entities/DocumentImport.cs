using Financist.Domain.Common;
using Financist.Domain.Enums;

namespace Financist.Domain.Entities;

public sealed class DocumentImport : AuditableEntity
{
    private DocumentImport()
    {
        StoredFileName = string.Empty;
        OriginalFileName = string.Empty;
        ContentType = string.Empty;
        StoragePath = string.Empty;
    }

    private DocumentImport(
        Guid userId,
        string storedFileName,
        string originalFileName,
        string contentType,
        string storagePath,
        long sizeBytes)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Document import user is required.");
        }

        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            throw new DomainException("Stored file name is required.");
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new DomainException("Original file name is required.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new DomainException("Document content type is required.");
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new DomainException("Document storage path is required.");
        }

        if (sizeBytes <= 0)
        {
            throw new DomainException("Document size must be greater than zero.");
        }

        UserId = userId;
        StoredFileName = storedFileName.Trim();
        OriginalFileName = originalFileName.Trim();
        ContentType = contentType.Trim();
        StoragePath = storagePath.Trim();
        SizeBytes = sizeBytes;
        Status = DocumentImportStatus.Pending;
        UploadedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string StoredFileName { get; private set; }

    public string OriginalFileName { get; private set; }

    public string ContentType { get; private set; }

    public string StoragePath { get; private set; }

    public long SizeBytes { get; private set; }

    public DocumentImportStatus Status { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime UploadedAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public User? User { get; private set; }

    public static DocumentImport Create(
        Guid userId,
        string storedFileName,
        string originalFileName,
        string contentType,
        string storagePath,
        long sizeBytes)
    {
        return new DocumentImport(userId, storedFileName, originalFileName, contentType, storagePath, sizeBytes);
    }

    public void MarkProcessing()
    {
        EnsureTransitionAllowed(DocumentImportStatus.Processing);
        Status = DocumentImportStatus.Processing;
        ErrorMessage = null;
        Touch();
    }

    public void MarkCompleted()
    {
        EnsureTransitionAllowed(DocumentImportStatus.Completed);
        Status = DocumentImportStatus.Completed;
        ProcessedAtUtc = DateTime.UtcNow;
        ErrorMessage = null;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Document import failure reason is required.");
        }

        EnsureTransitionAllowed(DocumentImportStatus.Failed);
        Status = DocumentImportStatus.Failed;
        ErrorMessage = reason.Trim();
        ProcessedAtUtc = DateTime.UtcNow;
        Touch();
    }

    private void EnsureTransitionAllowed(DocumentImportStatus targetStatus)
    {
        var valid = (Status, targetStatus) switch
        {
            (DocumentImportStatus.Pending, DocumentImportStatus.Processing) => true,
            (DocumentImportStatus.Pending, DocumentImportStatus.Failed) => true,
            (DocumentImportStatus.Processing, DocumentImportStatus.Completed) => true,
            (DocumentImportStatus.Processing, DocumentImportStatus.Failed) => true,
            (DocumentImportStatus.Failed, DocumentImportStatus.Processing) => true,
            _ => false
        };

        if (!valid)
        {
            throw new DomainException($"Invalid document import state transition from {Status} to {targetStatus}.");
        }
    }
}
