using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Purchasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseDocumentFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DocumentDate",
                schema: "purchasing",
                table: "CheckedDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vat",
                schema: "purchasing",
                table: "CheckedDocuments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "WithoutVat");

            // Fill the new filter columns of saved documents from their canonical JSON.
            migrationBuilder.Sql("""
                UPDATE purchasing."CheckedDocuments"
                SET "Vat" = CASE ("PayloadJson"::jsonb ->> 'vat') WHEN 'with_vat' THEN 'WithVat' ELSE 'WithoutVat' END,
                    "DocumentDate" = CASE
                        WHEN ("PayloadJson"::jsonb ->> 'documentDate') ~ '^\d{4}-\d{2}-\d{2}$'
                        THEN ("PayloadJson"::jsonb ->> 'documentDate')::date
                    END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CheckedDocuments_DocumentDate",
                schema: "purchasing",
                table: "CheckedDocuments",
                column: "DocumentDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckedDocuments_DocumentDate",
                schema: "purchasing",
                table: "CheckedDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentDate",
                schema: "purchasing",
                table: "CheckedDocuments");

            migrationBuilder.DropColumn(
                name: "Vat",
                schema: "purchasing",
                table: "CheckedDocuments");
        }
    }
}
