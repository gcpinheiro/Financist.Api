using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id).HasColumnName("id");
        builder.Property(transaction => transaction.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(transaction => transaction.CategoryId).HasColumnName("category_id");
        builder.Property(transaction => transaction.CardId).HasColumnName("card_id");

        builder.Property(transaction => transaction.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(transaction => transaction.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(transaction => transaction.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.Property(transaction => transaction.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(transaction => transaction.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(transaction => transaction.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.OwnsOne(transaction => transaction.Amount, money =>
        {
            money.Property(value => value.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(value => value.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(transaction => transaction.Amount).IsRequired();

        builder.HasOne(transaction => transaction.User)
            .WithMany()
            .HasForeignKey(transaction => transaction.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(transaction => transaction.Category)
            .WithMany()
            .HasForeignKey(transaction => transaction.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(transaction => transaction.Card)
            .WithMany()
            .HasForeignKey(transaction => transaction.CardId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
