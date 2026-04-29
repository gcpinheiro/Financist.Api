using Financist.Domain.Common;

namespace Financist.Domain.Entities;

public sealed class DocumentChunk : AuditableEntity
{
    private DocumentChunk()
    {
        Content = string.Empty;
    }

    private DocumentChunk(
        Guid documentImportId,
        Guid userId,
        int chunkIndex,
        string content)
    {
        if (documentImportId == Guid.Empty)
        {
            throw new DomainException("Document import is required.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("Document chunk user is required.");
        }

        if (chunkIndex < 0)
        {
            throw new DomainException("Document chunk index must be zero or greater.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Document chunk content is required.");
        }

        DocumentImportId = documentImportId;
        UserId = userId;
        ChunkIndex = chunkIndex;
        Content = content.Trim();
    }

    public Guid DocumentImportId { get; private set; }

    public Guid UserId { get; private set; }

    public int ChunkIndex { get; private set; }

    public string Content { get; private set; }

    public DocumentImport? DocumentImport { get; private set; }

    public User? User { get; private set; }

    public static DocumentChunk Create(
        Guid documentImportId,
        Guid userId,
        int chunkIndex,
        string content)
    {
        return new DocumentChunk(documentImportId, userId, chunkIndex, content);
    }
}
