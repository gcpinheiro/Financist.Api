using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Financist.Domain.Enums;

namespace Financist.Application.Features.Documents;

public sealed class DocumentTransactionExtractionService : IDocumentTransactionExtractionService
{
    private static readonly Regex DateRegex = new(
        @"(?<!\d)(?<day>\d{1,2})[\/\-.](?<month>\d{1,2})(?:[\/\-.](?<year>\d{2,4}))?(?!\d)",
        RegexOptions.Compiled);

    private static readonly Regex AmountAtEndRegex = new(
        @"(?<sign>[-+])?\s*(?:R\$\s*)?(?<amount>\d{1,3}(?:\.\d{3})*,\d{2}|\d+,\d{2}|\d+\.\d{2})\s*(?<credit>CR|C)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex InstallmentRegex = new(
        @"(?<!\d)(?:PARC(?:ELA)?\s*)?(?<number>\d{1,2})\s*(?:/|DE)\s*(?<count>\d{1,2})(?!\d)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly string[] IncomeTerms =
    [
        "cashback",
        "credito",
        "credit",
        "estorno",
        "pagamento",
        "reembolso"
    ];

    public IReadOnlyList<ExtractedDocumentTransactionCandidate> Extract(DocumentTransactionExtractionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return [];
        }

        var candidates = new List<ExtractedDocumentTransactionCandidate>();
        var occurrencesByBaseFingerprint = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var rawLine in SplitLines(request.Text))
        {
            var parsed = TryParseLine(rawLine, request.ReferenceDate, request.DefaultCurrency);
            if (parsed is null)
            {
                continue;
            }

            var baseFingerprint = BuildBaseFingerprint(parsed);
            occurrencesByBaseFingerprint.TryGetValue(baseFingerprint, out var occurrence);
            occurrence++;
            occurrencesByBaseFingerprint[baseFingerprint] = occurrence;

            var importFingerprint = Hash($"{baseFingerprint}|occurrence:{occurrence}");
            var installmentGroupKey = parsed.InstallmentNumber.HasValue
                ? Hash($"installment-group|{parsed.Type}|{parsed.Currency}|{NormalizeForKey(RemoveInstallmentMarker(parsed.Description))}|{parsed.Amount:0.00}|{parsed.InstallmentCount}")
                : null;

            candidates.Add(new ExtractedDocumentTransactionCandidate(
                Truncate(parsed.Description, 200),
                parsed.Amount,
                parsed.Currency,
                parsed.Type,
                parsed.OccurredOn,
                Truncate(rawLine, 1000),
                parsed.InstallmentNumber.HasValue ? 0.92m : 0.78m,
                parsed.InstallmentNumber,
                parsed.InstallmentCount,
                installmentGroupKey,
                importFingerprint));
        }

        return candidates;
    }

    private static ParsedTransactionLine? TryParseLine(
        string rawLine,
        DateOnly referenceDate,
        string defaultCurrency)
    {
        var line = NormalizeSpaces(rawLine);
        if (line.Length < 8)
        {
            return null;
        }

        var dateMatch = DateRegex.Match(line);
        var amountMatch = AmountAtEndRegex.Match(line);
        if (!dateMatch.Success || !amountMatch.Success || amountMatch.Index <= dateMatch.Index)
        {
            return null;
        }

        if (!TryParseDate(dateMatch, referenceDate, out var occurredOn) ||
            !TryParseAmount(amountMatch.Groups["amount"].Value, out var amount))
        {
            return null;
        }

        var descriptionStart = dateMatch.Index + dateMatch.Length;
        var descriptionLength = amountMatch.Index - descriptionStart;
        if (descriptionLength <= 0)
        {
            return null;
        }

        var description = NormalizeDescription(line.Substring(descriptionStart, descriptionLength));
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var installment = TryParseInstallment(description);
        var type = ResolveType(line, amountMatch);

        return new ParsedTransactionLine(
            description,
            Math.Abs(amount),
            NormalizeCurrency(defaultCurrency),
            type,
            occurredOn,
            installment.Number,
            installment.Count);
    }

    private static IReadOnlyList<string> SplitLines(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeSpaces)
            .Where(line => line.Length > 0)
            .ToArray();
    }

    private static string BuildBaseFingerprint(ParsedTransactionLine parsed)
    {
        var descriptionKey = NormalizeForKey(RemoveInstallmentMarker(parsed.Description));
        return string.Join(
            '|',
            "transaction-candidate-v1",
            parsed.Type,
            parsed.OccurredOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            parsed.Currency,
            parsed.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            descriptionKey,
            parsed.InstallmentNumber?.ToString(CultureInfo.InvariantCulture) ?? "single",
            parsed.InstallmentCount?.ToString(CultureInfo.InvariantCulture) ?? "single");
    }

    private static TransactionType ResolveType(string line, Match amountMatch)
    {
        if (string.Equals(amountMatch.Groups["sign"].Value, "-", StringComparison.Ordinal))
        {
            return TransactionType.Expense;
        }

        if (!string.IsNullOrWhiteSpace(amountMatch.Groups["credit"].Value))
        {
            return TransactionType.Income;
        }

        var normalizedLine = RemoveDiacritics(line).ToLowerInvariant();
        return IncomeTerms.Any(term => normalizedLine.Contains(term, StringComparison.Ordinal))
            ? TransactionType.Income
            : TransactionType.Expense;
    }

    private static (int? Number, int? Count) TryParseInstallment(string description)
    {
        var match = InstallmentRegex.Match(description);
        if (!match.Success ||
            !int.TryParse(match.Groups["number"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ||
            !int.TryParse(match.Groups["count"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var count))
        {
            return (null, null);
        }

        if (number <= 0 || count <= 0 || number > count)
        {
            return (null, null);
        }

        return (number, count);
    }

    private static bool TryParseDate(Match dateMatch, DateOnly referenceDate, out DateOnly date)
    {
        date = default;

        if (!int.TryParse(dateMatch.Groups["day"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var day) ||
            !int.TryParse(dateMatch.Groups["month"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var month))
        {
            return false;
        }

        var yearGroup = dateMatch.Groups["year"];
        var year = referenceDate.Year;

        if (yearGroup.Success)
        {
            if (!int.TryParse(yearGroup.Value, NumberStyles.None, CultureInfo.InvariantCulture, out year))
            {
                return false;
            }

            if (year < 100)
            {
                year += 2000;
            }
        }
        else if (month > referenceDate.Month + 1)
        {
            year--;
        }

        try
        {
            date = new DateOnly(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private static bool TryParseAmount(string value, out decimal amount)
    {
        var normalized = value.Trim();
        if (normalized.Contains(',', StringComparison.Ordinal))
        {
            normalized = normalized.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        }

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out amount);
    }

    private static string NormalizeDescription(string description)
    {
        return NormalizeSpaces(description)
            .Trim('-', '*', '.', ' ');
    }

    private static string RemoveInstallmentMarker(string description)
    {
        return NormalizeSpaces(InstallmentRegex.Replace(description, string.Empty));
    }

    private static string NormalizeForKey(string value)
    {
        return NormalizeSpaces(RemoveDiacritics(value))
            .ToLowerInvariant()
            .Trim();
    }

    private static string NormalizeSpaces(string value)
    {
        return WhitespaceRegex.Replace(value.Trim(), " ");
    }

    private static string NormalizeCurrency(string currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "BRL"
            : currency.Trim().ToUpperInvariant();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength].Trim();
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed record ParsedTransactionLine(
        string Description,
        decimal Amount,
        string Currency,
        TransactionType Type,
        DateOnly OccurredOn,
        int? InstallmentNumber,
        int? InstallmentCount);
}
