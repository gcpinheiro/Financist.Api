namespace Financist.Infrastructure.AI.Rag;

public sealed class RagOptions
{
    public const string SectionName = "Rag";

    public bool Enabled { get; set; } = true;

    public int MaxChunks { get; set; } = 5;

    public int MaxContextCharacters { get; set; } = 6_000;
}
