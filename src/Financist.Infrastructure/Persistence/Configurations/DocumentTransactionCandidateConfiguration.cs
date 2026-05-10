using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class DocumentTransactionCandidateConfiguration : IEntityTypeConfiguration<DocumentTransactionCandidate>
{
    public void Configure(EntityTypeBuilder<DocumentTransactionCandidate> builder)
    {
        builder.ToTable("document_transaction_candidates");

        builder.HasKey(candidate => candidate.Id);

        builder.Property(candidate => candidate.Id).HasColumnName("id");
        builder.Property(candidate => candidate.DocumentImportId).HasColumnName("document_import_id").IsRequired();
        builder.Property(candidate => candidate.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(candidate => candidate.TransactionId).HasColumnName("transaction_id");

        builder.Property(candidate => candidate.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(candidate => candidate.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(candidate => candidate.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(candidate => candidate.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.Property(candidate => candidate.RawText)
            .HasColumnName("raw_text")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(candidate => candidate.Confidence)
            .HasColumnName("confidence")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(candidate => candidate.InstallmentNumber)
            .HasColumnName("installment_number");

        builder.Property(candidate => candidate.InstallmentCount)
            .HasColumnName("installment_count");

        builder.Property(candidate => candidate.InstallmentGroupKey)
            .HasColumnName("installment_group_key")
            .HasMaxLength(64);

        builder.Property(candidate => candidate.ImportFingerprint)
            .HasColumnName("import_fingerprint")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(candidate => candidate.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(candidate => candidate.ImportedAtUtc)
            .HasColumnName("imported_at_utc");

        builder.Property(candidate => candidate.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(candidate => candidate.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.OwnsOne(candidate => candidate.Amount, money =>
        {
            money.Property(value => value.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(value => value.Currency)
                .HasColumnName("amount_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(candidate => candidate.Amount).IsRequired();

        builder.HasIndex(candidate => new { candidate.UserId, candidate.ImportFingerprint })
            .IsUnique();

        builder.HasIndex(candidate => new { candidate.DocumentImportId, candidate.Status });

        builder.HasIndex(candidate => candidate.InstallmentGroupKey);

        builder.HasOne(candidate => candidate.DocumentImport)
            .WithMany()
            .HasForeignKey(candidate => candidate.DocumentImportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(candidate => candidate.User)
            .WithMany()
            .HasForeignKey(candidate => candidate.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(candidate => candidate.Transaction)
            .WithMany()
            .HasForeignKey(candidate => candidate.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
