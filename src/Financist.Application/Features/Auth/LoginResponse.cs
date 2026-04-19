namespace Financist.Application.Features.Auth;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string FullName, string Email);
