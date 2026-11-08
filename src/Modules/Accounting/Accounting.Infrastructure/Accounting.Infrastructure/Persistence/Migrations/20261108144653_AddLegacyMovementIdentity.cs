using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyMovementIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "accounting",
                table: "Transfers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                schema: "accounting",
                table: "Transfers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankPurpose",
                schema: "accounting",
                table: "Transactions",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankTransactionId",
                schema: "accounting",
                table: "Transactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "accounting",
                table: "Transactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                schema: "accounting",
                table: "Transactions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ExternalId",
                schema: "accounting",
                table: "Transfers",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Fingerprint",
                schema: "accounting",
                table: "Transfers",
                column: "Fingerprint");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ExternalId",
                schema: "accounting",
                table: "Transactions",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Fingerprint",
                schema: "accounting",
                table: "Transactions",
                column: "Fingerprint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transfers_ExternalId",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_Fingerprint",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ExternalId",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Fingerprint",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Fingerprint",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "BankPurpose",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BankTransactionId",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Fingerprint",
                schema: "accounting",
                table: "Transactions");
        }
    }
}
