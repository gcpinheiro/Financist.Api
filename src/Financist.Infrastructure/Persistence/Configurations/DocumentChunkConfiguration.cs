using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financist.Infrastructure.Persistence.Configurations;

public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks");

        builder.HasKey(documentChunk => documentChunk.Id);

        builder.Property(documentChunk => documentChunk.Id).HasColumnName("id");
        builder.Property(documentChunk => documentChunk.DocumentImportId).HasColumnName("document_import_id").IsRequired();
        builder.Property(documentChunk => documentChunk.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(documentChunk => documentChunk.ChunkIndex).HasColumnName("chunk_index").IsRequired();

        builder.Property(documentChunk => documentChunk.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(documentChunk => documentChunk.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(documentChunk => documentChunk.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasIndex(documentChunk => new { documentChunk.DocumentImportId, documentChunk.ChunkIndex })
            .IsUnique();

        builder.HasIndex(documentChunk => documentChunk.UserId);

        builder.HasOne(documentChunk => documentChunk.DocumentImport)
            .WithMany()
            .HasForeignKey(documentChunk => documentChunk.DocumentImportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(documentChunk => documentChunk.User)
            .WithMany()
            .HasForeignKey(documentChunk => documentChunk.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
