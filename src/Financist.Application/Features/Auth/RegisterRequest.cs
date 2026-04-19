namespace Financist.Application.Features.Auth;

public sealed record RegisterRequest(string FullName, string Email, string Password);
