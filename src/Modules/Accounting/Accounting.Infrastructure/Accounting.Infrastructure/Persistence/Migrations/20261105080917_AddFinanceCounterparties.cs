using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceCounterparties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CounterpartyId",
                schema: "accounting",
                table: "Transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CounterpartyKind",
                schema: "accounting",
                table: "Transactions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Counterparties",
                schema: "accounting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Edrpou = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Iban = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BankName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Aliases = table.Column<string>(type: "character varying(5020)", maxLength: 5020, nullable: false),
                    DefaultCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AutoApplyCategory = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    SearchText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Verified = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    MergedIntoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counterparties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Counterparties_Categories_DefaultCategoryId",
                        column: x => x.DefaultCategoryId,
                        principalSchema: "accounting",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Counterparties_Counterparties_MergedIntoId",
                        column: x => x.MergedIntoId,
                        principalSchema: "accounting",
                        principalTable: "Counterparties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CounterpartyKind_CounterpartyId",
                schema: "accounting",
                table: "Transactions",
                columns: new[] { "CounterpartyKind", "CounterpartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Verification_Date",
                schema: "accounting",
                table: "Transactions",
                columns: new[] { "Verification", "Date" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Counterparty",
                schema: "accounting",
                table: "Transactions",
                sql: "(\"CounterpartyKind\" IS NULL) = (\"CounterpartyId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Active_Name",
                schema: "accounting",
                table: "Counterparties",
                columns: new[] { "Active", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_DefaultCategoryId",
                schema: "accounting",
                table: "Counterparties",
                column: "DefaultCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Edrpou",
                schema: "accounting",
                table: "Counterparties",
                column: "Edrpou");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Iban",
                schema: "accounting",
                table: "Counterparties",
                column: "Iban");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_MergedIntoId",
                schema: "accounting",
                table: "Counterparties",
                column: "MergedIntoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counterparties",
                schema: "accounting");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CounterpartyKind_CounterpartyId",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Verification_Date",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Counterparty",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CounterpartyKind",
                schema: "accounting",
                table: "Transactions");
        }
    }
}
