using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReviewMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The sheet copied this note to every shared line; the client asked to clear it (feedback item 11).
            migrationBuilder.Sql("""
                UPDATE catalog."SewingMaterialLines" SET "Comment" = NULL
                WHERE btrim("Comment") = 'Спільна складова для всіх артикулів моделі';
                UPDATE catalog."SewingOperationLines" SET "Comment" = NULL
                WHERE btrim("Comment") = 'Спільна складова для всіх артикулів моделі';
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_SewingOperationLines_ReviewMetadata",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SewingMaterialLines_ReviewMetadata",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropColumn(
                name: "IsReviewed",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.DropColumn(
                name: "IsReviewed",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.DropColumn(
                name: "IsSourceReviewed",
                schema: "catalog",
                table: "ProductCatalogProfiles");

            migrationBuilder.DropColumn(
                name: "SourceReviewDate",
                schema: "catalog",
                table: "ProductCatalogProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                schema: "catalog",
                table: "SewingOperationLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReviewed",
                schema: "catalog",
                table: "SewingOperationLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "catalog",
                table: "SewingOperationLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                schema: "catalog",
                table: "SewingOperationLines",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                schema: "catalog",
                table: "SewingMaterialLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReviewed",
                schema: "catalog",
                table: "SewingMaterialLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "catalog",
                table: "SewingMaterialLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                schema: "catalog",
                table: "SewingMaterialLines",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSourceReviewed",
                schema: "catalog",
                table: "ProductCatalogProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SourceReviewDate",
                schema: "catalog",
                table: "ProductCatalogProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SewingOperationLines_ReviewMetadata",
                schema: "catalog",
                table: "SewingOperationLines",
                sql: "NOT \"IsReviewed\" OR (\"ReviewedBy\" IS NOT NULL AND length(trim(\"ReviewedBy\")) > 0 AND \"ReviewedAt\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SewingMaterialLines_ReviewMetadata",
                schema: "catalog",
                table: "SewingMaterialLines",
                sql: "NOT \"IsReviewed\" OR (\"ReviewedBy\" IS NOT NULL AND length(trim(\"ReviewedBy\")) > 0 AND \"ReviewedAt\" IS NOT NULL)");
        }
    }
}
