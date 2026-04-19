using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).HasColumnName("id");
        builder.Property(category => category.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(category => category.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(category => category.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(category => category.IsSystem)
            .HasColumnName("is_system")
            .IsRequired();

        builder.Property(category => category.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(category => category.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasOne(category => category.User)
            .WithMany()
            .HasForeignKey(category => category.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(category => new { category.UserId, category.Name, category.Type })
            .IsUnique();
    }
}
