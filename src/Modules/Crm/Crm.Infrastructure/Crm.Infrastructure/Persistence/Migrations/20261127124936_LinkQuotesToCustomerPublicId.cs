using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkQuotesToCustomerPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerPublicId",
                schema: "crm",
                table: "Quotes",
                type: "uuid",
                nullable: true);

            // CARD-001 R-02: КП already linked by the internal id (old КП moved from the calculator) get the same
            // customer's public id, so the client card lists them.
            migrationBuilder.Sql("""
                UPDATE crm."Quotes" AS q
                SET "CustomerPublicId" = c."PublicId"
                FROM crm."Customers" AS c
                WHERE q."CustomerId" = c."Id" AND q."CustomerPublicId" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CustomerPublicId_CreatedAt",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "CustomerPublicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTasks_CustomerPublicId_CreatedAt",
                schema: "crm",
                table: "ManagerTasks",
                columns: new[] { "CustomerPublicId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quotes_CustomerPublicId_CreatedAt",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_ManagerTasks_CustomerPublicId_CreatedAt",
                schema: "crm",
                table: "ManagerTasks");

            migrationBuilder.DropColumn(
                name: "CustomerPublicId",
                schema: "crm",
                table: "Quotes");
        }
    }
}
