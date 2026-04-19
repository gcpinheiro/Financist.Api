namespace Financist.Application.Features.Documents;

public sealed class UploadDocumentRequest
{
    public required Stream Content { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required long SizeBytes { get; init; }
}
