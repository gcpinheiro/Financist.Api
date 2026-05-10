using Financist.Application.Features.Documents;

namespace Financist.UnitTests.Application;

public sealed class DocumentTransactionExtractionServiceTests
{
    private readonly DocumentTransactionExtractionService _service = new();

    [Fact]
    public void Extract_InstallmentsFromDifferentStatements_ShouldShareGroupButUseDifferentFingerprints()
    {
        var firstStatement = Extract("""
            10/03 LOJA EXEMPLO 02/10 R$ 123,45
            """);

        var nextStatement = Extract("""
            10/04 LOJA EXEMPLO 03/10 R$ 123,45
            """);

        var firstCandidate = Assert.Single(firstStatement);
        var nextCandidate = Assert.Single(nextStatement);

        Assert.Equal(2, firstCandidate.InstallmentNumber);
        Assert.Equal(3, nextCandidate.InstallmentNumber);
        Assert.Equal(10, firstCandidate.InstallmentCount);
        Assert.Equal(firstCandidate.InstallmentGroupKey, nextCandidate.InstallmentGroupKey);
        Assert.NotEqual(firstCandidate.ImportFingerprint, nextCandidate.ImportFingerprint);
    }

    [Fact]
    public void Extract_SameStatementAgain_ShouldKeepSameFingerprint()
    {
        const string text = "10/03 LOJA EXEMPLO 02/10 R$ 123,45";

        var firstCandidate = Assert.Single(Extract(text));
        var repeatedCandidate = Assert.Single(Extract(text));

        Assert.Equal(firstCandidate.ImportFingerprint, repeatedCandidate.ImportFingerprint);
        Assert.Equal(firstCandidate.InstallmentGroupKey, repeatedCandidate.InstallmentGroupKey);
    }

    [Fact]
    public void Extract_IdenticalRowsInSameStatement_ShouldKeepSeparateFingerprints()
    {
        var candidates = Extract("""
            10/03 CAFE EXEMPLO R$ 12,00
            10/03 CAFE EXEMPLO R$ 12,00
            """);

        Assert.Equal(2, candidates.Count);
        Assert.NotEqual(candidates[0].ImportFingerprint, candidates[1].ImportFingerprint);
    }

    private IReadOnlyList<ExtractedDocumentTransactionCandidate> Extract(string text)
    {
        return _service.Extract(new DocumentTransactionExtractionRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            text,
            new DateOnly(2026, 4, 29),
            "BRL"));
    }
}
