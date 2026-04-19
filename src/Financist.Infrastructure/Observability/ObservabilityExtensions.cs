using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Financist.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddFinancistObservability(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(FinancistTelemetry.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(FinancistTelemetry.ActivitySource.Name)
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.statement", command.CommandText);
                        };
                    });

                var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();

                var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}
