using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierKeepinCrmFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "reference",
                table: "Suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "reference",
                table: "Suppliers",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "reference",
                table: "Suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "KeepinCrmId",
                schema: "reference",
                table: "Suppliers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "KeepinSyncedAt",
                schema: "reference",
                table: "Suppliers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseCategories",
                schema: "reference",
                table: "Suppliers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                schema: "reference",
                table: "Suppliers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_KeepinCrmId",
                schema: "reference",
                table: "Suppliers",
                column: "KeepinCrmId",
                unique: true,
                filter: "\"KeepinCrmId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_KeepinCrmId",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "KeepinCrmId",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "KeepinSyncedAt",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PurchaseCategories",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "reference",
                table: "Suppliers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "reference",
                table: "Suppliers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
