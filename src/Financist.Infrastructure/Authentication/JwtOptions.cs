namespace Financist.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Financist";

    public string Audience { get; set; } = "Financist.Client";

    public string Key { get; set; } = "super-secret-development-key-change-me";

    public int ExpirationMinutes { get; set; } = 60;
}
