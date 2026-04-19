namespace Financist.Infrastructure.Services;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string DocumentsPath { get; set; } = "storage/documents";
}
