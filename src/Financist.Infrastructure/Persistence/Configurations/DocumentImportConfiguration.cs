using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class DocumentImportConfiguration : IEntityTypeConfiguration<DocumentImport>
{
    public void Configure(EntityTypeBuilder<DocumentImport> builder)
    {
        builder.ToTable("document_imports");

        builder.HasKey(documentImport => documentImport.Id);

        builder.Property(documentImport => documentImport.Id).HasColumnName("id");
        builder.Property(documentImport => documentImport.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(documentImport => documentImport.StoredFileName)
            .HasColumnName("stored_file_name")
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(documentImport => documentImport.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(documentImport => documentImport.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(documentImport => documentImport.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(documentImport => documentImport.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(documentImport => documentImport.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(documentImport => documentImport.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(documentImport => documentImport.UploadedAtUtc)
            .HasColumnName("uploaded_at_utc")
            .IsRequired();

        builder.Property(documentImport => documentImport.ProcessedAtUtc)
            .HasColumnName("processed_at_utc");

        builder.Property(documentImport => documentImport.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(documentImport => documentImport.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasOne(documentImport => documentImport.User)
            .WithMany()
            .HasForeignKey(documentImport => documentImport.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
