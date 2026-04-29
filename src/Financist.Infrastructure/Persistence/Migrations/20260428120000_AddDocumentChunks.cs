using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financist.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_chunks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_import_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_chunks", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_chunks_document_imports_document_import_id",
                        column: x => x.document_import_id,
                        principalTable: "document_imports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_chunks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_document_import_id_chunk_index",
                table: "document_chunks",
                columns: new[] { "document_import_id", "chunk_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_user_id",
                table: "document_chunks",
                column: "user_id");

            migrationBuilder.Sql(
                "CREATE INDEX IX_document_chunks_content_fts ON document_chunks USING GIN (to_tsvector('portuguese', content));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_document_chunks_content_fts;");

            migrationBuilder.DropTable(
                name: "document_chunks");
        }
    }
}
