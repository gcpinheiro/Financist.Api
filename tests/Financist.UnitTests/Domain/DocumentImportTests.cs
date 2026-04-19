using Financist.Domain.Common;
using Financist.Domain.Entities;
using Financist.Domain.Enums;

namespace Financist.UnitTests.Domain;

public sealed class DocumentImportTests
{
    [Fact]
    public void MarkProcessing_ThenMarkCompleted_ShouldChangeStatus()
    {
        var documentImport = DocumentImport.Create(
            Guid.NewGuid(),
            "stored.pdf",
            "statement.pdf",
            "application/pdf",
            "2026/04/stored.pdf",
            1024);

        documentImport.MarkProcessing();
        documentImport.MarkCompleted();

        Assert.Equal(DocumentImportStatus.Completed, documentImport.Status);
        Assert.NotNull(documentImport.ProcessedAtUtc);
    }

    [Fact]
    public void MarkCompleted_FromPending_ShouldThrow()
    {
        var documentImport = DocumentImport.Create(
            Guid.NewGuid(),
            "stored.pdf",
            "statement.pdf",
            "application/pdf",
            "2026/04/stored.pdf",
            1024);

        var action = () => documentImport.MarkCompleted();

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("Invalid document import state transition", exception.Message);
    }
}
