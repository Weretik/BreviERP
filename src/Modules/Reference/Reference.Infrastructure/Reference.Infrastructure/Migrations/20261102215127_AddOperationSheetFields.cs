using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationSheetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Element",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MinutesPerProduct",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NormCheckedAt",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Variant",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentPartOperations_Code",
                schema: "reference",
                table: "GarmentPartOperations",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GarmentPartOperations_Code",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "Element",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "MinutesPerProduct",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "NormCheckedAt",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "reference",
                table: "GarmentPartOperations");

            migrationBuilder.DropColumn(
                name: "Variant",
                schema: "reference",
                table: "GarmentPartOperations");
        }
    }
}
