using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(goal => goal.Id);

        builder.Property(goal => goal.Id).HasColumnName("id");
        builder.Property(goal => goal.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(goal => goal.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(goal => goal.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(goal => goal.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(goal => goal.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.OwnsOne(goal => goal.TargetAmount, money =>
        {
            money.Property(value => value.Amount)
                .HasColumnName("target_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(value => value.Currency)
                .HasColumnName("target_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(goal => goal.CurrentAmount, money =>
        {
            money.Property(value => value.Amount)
                .HasColumnName("current_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(value => value.Currency)
                .HasColumnName("current_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(goal => goal.TargetAmount).IsRequired();
        builder.Navigation(goal => goal.CurrentAmount).IsRequired();

        builder.HasOne(goal => goal.User)
            .WithMany()
            .HasForeignKey(goal => goal.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
