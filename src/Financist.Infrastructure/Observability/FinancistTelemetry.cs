using System.Diagnostics;

namespace Financist.Infrastructure.Observability;

public static class FinancistTelemetry
{
    public const string ServiceName = "Financist.Api";

    public static readonly ActivitySource ActivitySource = new("Financist.Application");
}
