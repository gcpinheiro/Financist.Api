using Financist.Application.Abstractions.Authentication;
using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Abstractions.Storage;
using Financist.Infrastructure.Authentication;
using Financist.Infrastructure.Observability;
using Financist.Infrastructure.Persistence;
using Financist.Infrastructure.Persistence.Repositories;
using Financist.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Financist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        services.AddDbContext<FinancistDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<FinancistDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IDocumentImportRepository, DocumentImportRepository>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDocumentStorageService, LocalDocumentStorageService>();
        services.AddSingleton<IDocumentAnalysisService, NullDocumentAnalysisService>();

        services.AddHealthChecks()
            .AddDbContextCheck<FinancistDbContext>("postgresql");

        services.AddFinancistObservability(configuration);

        return services;
    }
}
