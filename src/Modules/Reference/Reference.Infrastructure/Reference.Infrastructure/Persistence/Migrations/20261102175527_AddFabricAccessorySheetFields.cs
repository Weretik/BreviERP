using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricAccessorySheetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GarmentAccessoriesReference_Name",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropIndex(
                name: "IX_FabricsReference_Name",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PriceUpdatedAt",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "шт.");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "reference",
                table: "FabricsReference",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "reference",
                table: "FabricsReference",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompositionRu",
                schema: "reference",
                table: "FabricsReference",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "reference",
                table: "FabricsReference",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                schema: "reference",
                table: "FabricsReference",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PriceUpdatedAt",
                schema: "reference",
                table: "FabricsReference",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposalDescription",
                schema: "reference",
                table: "FabricsReference",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentAccessoriesReference_Code",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentAccessoriesReference_Name",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_FabricsReference_Code",
                schema: "reference",
                table: "FabricsReference",
                column: "Code",
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FabricsReference_Name",
                schema: "reference",
                table: "FabricsReference",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GarmentAccessoriesReference_Code",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropIndex(
                name: "IX_GarmentAccessoriesReference_Name",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropIndex(
                name: "IX_FabricsReference_Code",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropIndex(
                name: "IX_FabricsReference_Name",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropColumn(
                name: "PriceUpdatedAt",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "reference",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "CompositionRu",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "NameRu",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "PriceUpdatedAt",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.DropColumn(
                name: "ProposalDescription",
                schema: "reference",
                table: "FabricsReference");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentAccessoriesReference_Name",
                schema: "reference",
                table: "GarmentAccessoriesReference",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricsReference_Name",
                schema: "reference",
                table: "FabricsReference",
                column: "Name",
                unique: true);
        }
    }
}
