using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("cards");

        builder.HasKey(card => card.Id);

        builder.Property(card => card.Id).HasColumnName("id");
        builder.Property(card => card.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(card => card.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(card => card.Last4Digits)
            .HasColumnName("last4_digits")
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(card => card.ClosingDay)
            .HasColumnName("closing_day")
            .IsRequired();

        builder.Property(card => card.DueDay)
            .HasColumnName("due_day")
            .IsRequired();

        builder.Property(card => card.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(card => card.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.OwnsOne(card => card.Limit, money =>
        {
            money.Property(value => value.Amount)
                .HasColumnName("limit_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(value => value.Currency)
                .HasColumnName("limit_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(card => card.Limit).IsRequired();

        builder.HasOne(card => card.User)
            .WithMany()
            .HasForeignKey(card => card.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
