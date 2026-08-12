using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveReferenceToReferenceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ProductCategories\";");

            migrationBuilder.EnsureSchema(name: "reference");

            migrationBuilder.RenameTable(name: "Suppliers", newName: "Suppliers", newSchema: "reference");
            migrationBuilder.RenameTable(name: "GarmentParts", newName: "GarmentParts", newSchema: "reference");
            migrationBuilder.RenameTable(name: "GarmentPartOperations", newName: "GarmentPartOperations", newSchema: "reference");
            migrationBuilder.RenameTable(name: "GarmentAccessoriesReference", newName: "GarmentAccessoriesReference", newSchema: "reference");
            migrationBuilder.RenameTable(name: "FabricsReference", newName: "FabricsReference", newSchema: "reference");
            migrationBuilder.RenameTable(name: "AdditionalReferences", newName: "AdditionalReferences", newSchema: "reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Suppliers", schema: "reference", newName: "Suppliers", newSchema: "public");
            migrationBuilder.RenameTable(name: "GarmentParts", schema: "reference", newName: "GarmentParts", newSchema: "public");
            migrationBuilder.RenameTable(name: "GarmentPartOperations", schema: "reference", newName: "GarmentPartOperations", newSchema: "public");
            migrationBuilder.RenameTable(name: "GarmentAccessoriesReference", schema: "reference", newName: "GarmentAccessoriesReference", newSchema: "public");
            migrationBuilder.RenameTable(name: "FabricsReference", schema: "reference", newName: "FabricsReference", newSchema: "public");
            migrationBuilder.RenameTable(name: "AdditionalReferences", schema: "reference", newName: "AdditionalReferences", newSchema: "public");
        }
    }
}
