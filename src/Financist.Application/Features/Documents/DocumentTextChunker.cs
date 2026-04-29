namespace Financist.Application.Features.Documents;

public sealed class DocumentTextChunker
{
    private const int DefaultMaxChunkLength = 1_200;
    private const int DefaultOverlapLength = 150;

    public IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var normalizedText = NormalizeWhitespace(text);
        var chunks = new List<string>();
        var start = 0;

        while (start < normalizedText.Length)
        {
            var remainingLength = normalizedText.Length - start;
            var length = Math.Min(DefaultMaxChunkLength, remainingLength);

            if (length == DefaultMaxChunkLength)
            {
                var boundary = normalizedText.LastIndexOf(' ', start + length, length);
                if (boundary > start + (DefaultMaxChunkLength / 2))
                {
                    length = boundary - start;
                }
            }

            var chunk = normalizedText.Substring(start, length).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (start + length >= normalizedText.Length)
            {
                break;
            }

            start += Math.Max(1, length - DefaultOverlapLength);
        }

        return chunks;
    }

    private static string NormalizeWhitespace(string text)
    {
        var result = new char[text.Length];
        var writeIndex = 0;
        var previousWasWhitespace = false;

        foreach (var character in text)
        {
            if (char.IsWhiteSpace(character))
            {
                if (previousWasWhitespace)
                {
                    continue;
                }

                result[writeIndex++] = ' ';
                previousWasWhitespace = true;
                continue;
            }

            result[writeIndex++] = character;
            previousWasWhitespace = false;
        }

        return new string(result, 0, writeIndex).Trim();
    }
}
