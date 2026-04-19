namespace Financist.Application.Abstractions.Services;

public sealed record DocumentAnalysisRequest(string FileName, string ContentType, long SizeBytes);
