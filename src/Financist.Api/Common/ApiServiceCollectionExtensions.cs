using System.Text.Json.Serialization;
using Financist.Api.Common;
using Financist.Application.Abstractions.Services;
using Financist.Application.Features.Auth;
using Financist.Application.Features.Cards;
using Financist.Application.Features.Categories;
using Financist.Application.Features.Dashboard;
using Financist.Application.Features.Documents;
using Financist.Application.Features.Goals;
using Financist.Application.Features.Transactions;

namespace Financist.Api.Common;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddFinancistApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<DocumentTextChunker>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }
}
