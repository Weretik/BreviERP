using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankStatements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankStatements",
                schema: "accounting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    FirstDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    TransferCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BookedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BookedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatements_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "accounting",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankStatementLines",
                schema: "accounting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: true),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    BankName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Edrpou = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Iban = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    Document = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BankId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RawJson = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    Duplicate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DuplicateNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TransferAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CounterpartyKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CounterpartyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CounterpartyName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CounterpartyVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    OneoffName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    Excluded = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementLines", x => x.Id);
                    table.CheckConstraint("CK_BankStatementLines_Counterparty", "(\"CounterpartyKind\" IS NULL) = (\"CounterpartyId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_BankStatementLines_BankStatements_StatementId",
                        column: x => x.StatementId,
                        principalSchema: "accounting",
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_StatementId_RowNumber",
                schema: "accounting",
                table: "BankStatementLines",
                columns: new[] { "StatementId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_StatementId_State",
                schema: "accounting",
                table: "BankStatementLines",
                columns: new[] { "StatementId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_TransactionId",
                schema: "accounting",
                table: "BankStatementLines",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_TransferId",
                schema: "accounting",
                table: "BankStatementLines",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_AccountId_FileHash",
                schema: "accounting",
                table: "BankStatements",
                columns: new[] { "AccountId", "FileHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_CreatedAt",
                schema: "accounting",
                table: "BankStatements",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankStatementLines",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "BankStatements",
                schema: "accounting");
        }
    }
}
