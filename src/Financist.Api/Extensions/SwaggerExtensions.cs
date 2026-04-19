using Microsoft.OpenApi;

namespace Financist.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddFinancistSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Financist API",
                Version = "v1",
                Description = "Production-oriented backend foundation for personal finance management."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Provide the JWT access token."
            });
        });

        return services;
    }
}
