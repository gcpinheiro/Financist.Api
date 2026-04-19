using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Financist.Infrastructure.Persistence;

public sealed class FinancistDbContextFactory : IDesignTimeDbContextFactory<FinancistDbContext>
{
    public FinancistDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinancistDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            "Host=localhost;Port=5432;Database=financist;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);
        return new FinancistDbContext(optionsBuilder.Options);
    }
}
