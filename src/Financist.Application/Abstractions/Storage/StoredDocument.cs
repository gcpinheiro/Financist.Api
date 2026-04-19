namespace Financist.Application.Abstractions.Storage;

public sealed record StoredDocument(string StoredFileName, string StoragePath, long SizeBytes);
