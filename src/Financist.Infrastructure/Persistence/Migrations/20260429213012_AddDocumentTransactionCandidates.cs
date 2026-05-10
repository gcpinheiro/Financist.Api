using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financist.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTransactionCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_transaction_candidates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_import_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    occurred_on = table.Column<DateOnly>(type: "date", nullable: false),
                    raw_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    installment_number = table.Column<int>(type: "integer", nullable: true),
                    installment_count = table.Column<int>(type: "integer", nullable: true),
                    installment_group_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    import_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    imported_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_transaction_candidates", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_transaction_candidates_document_imports_document_i~",
                        column: x => x.document_import_id,
                        principalTable: "document_imports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_transaction_candidates_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_document_transaction_candidates_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_transaction_candidates_document_import_id_status",
                table: "document_transaction_candidates",
                columns: new[] { "document_import_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_document_transaction_candidates_installment_group_key",
                table: "document_transaction_candidates",
                column: "installment_group_key");

            migrationBuilder.CreateIndex(
                name: "IX_document_transaction_candidates_transaction_id",
                table: "document_transaction_candidates",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_transaction_candidates_user_id_import_fingerprint",
                table: "document_transaction_candidates",
                columns: new[] { "user_id", "import_fingerprint" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_transaction_candidates");
        }
    }
}
