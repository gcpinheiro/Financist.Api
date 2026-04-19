using Financist.Domain.Enums;

namespace Financist.Application.Features.Documents;

public sealed record DocumentImportDto(
    Guid Id,
    string StoredFileName,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DocumentImportStatus Status,
    DateTime UploadedAtUtc);
