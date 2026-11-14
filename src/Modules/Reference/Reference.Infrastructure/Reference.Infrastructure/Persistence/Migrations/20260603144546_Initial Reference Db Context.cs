using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialReferenceDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentAccessoriesReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentAccessoriesReference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentPartOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GarmentPartId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Min = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentPartOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricsReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricsReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricsReference_Suppliers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalReferences_Key",
                table: "AdditionalReferences",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalReferences_Name",
                table: "AdditionalReferences",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricsReference_Name",
                table: "FabricsReference",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FabricsReference_ProviderId",
                table: "FabricsReference",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentAccessoriesReference_Name",
                table: "GarmentAccessoriesReference",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentPartOperations_GarmentPartId_Name",
                table: "GarmentPartOperations",
                columns: new[] { "GarmentPartId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentParts_Name",
                table: "GarmentParts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalReferences");

            migrationBuilder.DropTable(
                name: "FabricsReference");

            migrationBuilder.DropTable(
                name: "GarmentAccessoriesReference");

            migrationBuilder.DropTable(
                name: "GarmentPartOperations");

            migrationBuilder.DropTable(
                name: "GarmentParts");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
