using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSupplierToGarmentAccessory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarmentParts_Suppliers_SupplierId",
                table: "GarmentParts");

            migrationBuilder.DropIndex(
                name: "IX_GarmentParts_SupplierId",
                table: "GarmentParts");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "GarmentParts");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "GarmentParts");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "GarmentParts");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "GarmentAccessoriesReference",
                type: "integer",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentAccessoriesReference_SupplierId",
                table: "GarmentAccessoriesReference",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_GarmentAccessoriesReference_Suppliers_SupplierId",
                table: "GarmentAccessoriesReference",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarmentAccessoriesReference_Suppliers_SupplierId",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropIndex(
                name: "IX_GarmentAccessoriesReference_SupplierId",
                table: "GarmentAccessoriesReference");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "GarmentAccessoriesReference");

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "GarmentParts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "GarmentParts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "GarmentParts",
                type: "integer",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentParts_SupplierId",
                table: "GarmentParts",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_GarmentParts_Suppliers_SupplierId",
                table: "GarmentParts",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
