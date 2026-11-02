using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PricingSettingsFromSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The old rows are kept (renamed) so nothing is lost on deploy; the sheet import replaces them
            // after a preview and the user's confirmation.
            // Names repeat in the sheet («Коефіцієнт ціни: до 10 шт.»), so only the code stays unique.
            migrationBuilder.Sql("""DROP INDEX IF EXISTS reference."IX_AdditionalReferences_Name";""");

            migrationBuilder.RenameTable(
                name: "AdditionalReferences",
                schema: "reference",
                newName: "PricingParameters",
                newSchema: "reference");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdditionalReferences",
                schema: "reference",
                table: "PricingParameters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricingParameters",
                schema: "reference",
                table: "PricingParameters",
                column: "Id");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "reference",
                table: "PricingParameters",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "reference",
                table: "PricingParameters",
                newName: "Comment");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalReferences_Key",
                schema: "reference",
                table: "PricingParameters",
                newName: "IX_PricingParameters_Code");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "reference",
                table: "PricingParameters",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "reference",
                table: "PricingParameters",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                schema: "reference",
                table: "PricingParameters",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                schema: "reference",
                table: "PricingParameters",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: "reference",
                table: "PricingParameters",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                schema: "reference",
                table: "PricingParameters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Старі довідники");

            migrationBuilder.AddColumn<bool>(
                name: "IsCalculated",
                schema: "reference",
                table: "PricingParameters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdatedOn",
                schema: "reference",
                table: "PricingParameters",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "reference",
                table: "PricingParameters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""UPDATE reference."PricingParameters" SET "SortOrder" = "Id";""");

            migrationBuilder.CreateTable(
                name: "PpeMarkupRules",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RetailMarkup = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    WholesaleMarkup = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    LargeWholesaleMarkup = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    PriceNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppliesTo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeMarkupRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PpeMarkupRules_Name",
                schema: "reference",
                table: "PpeMarkupRules",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpeMarkupRules",
                schema: "reference");

            migrationBuilder.Sql("""UPDATE reference."PricingParameters" SET "Value" = 0 WHERE "Value" IS NULL;""");

            migrationBuilder.DropColumn(name: "Group", schema: "reference", table: "PricingParameters");
            migrationBuilder.DropColumn(name: "IsCalculated", schema: "reference", table: "PricingParameters");
            migrationBuilder.DropColumn(name: "UpdatedOn", schema: "reference", table: "PricingParameters");
            migrationBuilder.DropColumn(name: "SortOrder", schema: "reference", table: "PricingParameters");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                schema: "reference",
                table: "PricingParameters",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.RenameColumn(name: "Code", schema: "reference", table: "PricingParameters", newName: "Key");
            migrationBuilder.RenameColumn(name: "Comment", schema: "reference", table: "PricingParameters", newName: "Description");
            migrationBuilder.RenameIndex(
                name: "IX_PricingParameters_Code",
                schema: "reference",
                table: "PricingParameters",
                newName: "IX_AdditionalReferences_Key");
            migrationBuilder.DropPrimaryKey(name: "PK_PricingParameters", schema: "reference", table: "PricingParameters");
            migrationBuilder.RenameTable(
                name: "PricingParameters",
                schema: "reference",
                newName: "AdditionalReferences",
                newSchema: "reference");
            migrationBuilder.AddPrimaryKey(name: "PK_AdditionalReferences", schema: "reference", table: "AdditionalReferences", column: "Id");
            // Text lengths stay wider on the way down: shortening could fail on imported rows.
        }
    }
}
