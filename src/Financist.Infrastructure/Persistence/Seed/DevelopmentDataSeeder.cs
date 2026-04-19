using Financist.Application.Abstractions.Authentication;
using Financist.Domain.Entities;
using Financist.Domain.Enums;
using Financist.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Seed;

public static class DevelopmentDataSeeder
{
    public const string DefaultEmail = "dev@financist.local";
    public const string DefaultPassword = "Financist123!";

    public static async Task SeedAsync(FinancistDbContext dbContext, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var user = User.Create("Financist Dev", DefaultEmail, passwordHasher.Hash(DefaultPassword));

        var salaryCategory = Category.Create(user.Id, "Salary", TransactionType.Income, true);
        var groceriesCategory = Category.Create(user.Id, "Groceries", TransactionType.Expense, true);
        var transportCategory = Category.Create(user.Id, "Transport", TransactionType.Expense, true);

        var card = Card.Create(user.Id, "Primary Card", "4242", new Money(3500m, "USD"), 8, 15);

        var goal = Goal.Create(
            user.Id,
            "Emergency Fund",
            "Build a safer cash reserve.",
            new Money(10000m, "USD"),
            new Money(1500m, "USD"));

        var salaryTransaction = Transaction.Create(
            user.Id,
            "Monthly salary",
            new Money(4200m, "USD"),
            TransactionType.Income,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            salaryCategory.Id,
            null,
            "Seeded development income.");

        var groceriesTransaction = Transaction.Create(
            user.Id,
            "Supermarket purchase",
            new Money(182.35m, "USD"),
            TransactionType.Expense,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            groceriesCategory.Id,
            card.Id,
            "Seeded development expense.");

        var transportTransaction = Transaction.Create(
            user.Id,
            "Ride sharing",
            new Money(26.40m, "USD"),
            TransactionType.Expense,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            transportCategory.Id,
            card.Id,
            "Seeded development expense.");

        var documentImport = DocumentImport.Create(
            user.Id,
            "seed-statement.pdf",
            "seed-statement.pdf",
            "application/pdf",
            "seed/seed-statement.pdf",
            2048);

        documentImport.MarkProcessing();
        documentImport.MarkCompleted();

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.Categories.AddRangeAsync([salaryCategory, groceriesCategory, transportCategory], cancellationToken);
        await dbContext.Cards.AddAsync(card, cancellationToken);
        await dbContext.Goals.AddAsync(goal, cancellationToken);
        await dbContext.Transactions.AddRangeAsync([salaryTransaction, groceriesTransaction, transportTransaction], cancellationToken);
        await dbContext.DocumentImports.AddAsync(documentImport, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
