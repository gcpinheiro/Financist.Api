using Financist.Application.Abstractions.Authentication;
using Financist.Infrastructure.Authentication;
using Financist.Infrastructure.Persistence;
using Financist.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Financist.IntegrationTests.Api;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"financist-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<FinancistDbContext>));
            services.RemoveAll(typeof(FinancistDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<FinancistDbContext>));

            services.AddDbContext<FinancistDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<FinancistDbContext>();
            var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher>();

            dbContext.Database.EnsureCreated();
            DevelopmentDataSeeder.SeedAsync(dbContext, passwordHasher).GetAwaiter().GetResult();
        });
    }
}
