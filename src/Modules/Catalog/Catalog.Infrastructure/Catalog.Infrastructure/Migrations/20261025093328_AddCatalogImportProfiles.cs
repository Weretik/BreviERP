using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogImportProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SewingOperationLines_ProductId_OperationId",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SewingMaterialLines_Reference",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.CreateTable(
                name: "ProductCatalogProfiles",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SaleSku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Season = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSourceReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    SourceReviewDate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    SupplierSku = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SupplierProductUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SupplierPrice = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    AllowManualPrice = table.Column<bool>(type: "boolean", nullable: false),
                    PricingRuleCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCatalogProfiles", x => x.ProductId);
                    table.CheckConstraint("CK_ProductCatalogProfiles_SupplierPrice", "\"SupplierPrice\" IS NULL OR \"SupplierPrice\" > 0");
                    table.ForeignKey(
                        name: "FK_ProductCatalogProfiles_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SewingOperationLines_ProductId_OperationId",
                schema: "catalog",
                table: "SewingOperationLines",
                columns: new[] { "ProductId", "OperationId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SewingMaterialLines_Reference",
                schema: "catalog",
                table: "SewingMaterialLines",
                sql: "(\"Kind\" = 1 AND \"FabricId\" > 0 AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NULL AND \"Unit\" = 1) OR (\"Kind\" = 2 AND \"FabricId\" IS NULL AND \"AccessoryId\" > 0 AND \"CostCode\" IS NULL AND \"Unit\" IN (1, 2)) OR (\"Kind\" = 3 AND \"FabricId\" IS NULL AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NOT NULL AND length(trim(\"CostCode\")) > 0 AND \"Unit\" = 3)");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCatalogProfiles_SaleSku",
                schema: "catalog",
                table: "ProductCatalogProfiles",
                column: "SaleSku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCatalogProfiles",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_SewingOperationLines_ProductId_OperationId",
                schema: "catalog",
                table: "SewingOperationLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SewingMaterialLines_Reference",
                schema: "catalog",
                table: "SewingMaterialLines");

            migrationBuilder.CreateIndex(
                name: "IX_SewingOperationLines_ProductId_OperationId",
                schema: "catalog",
                table: "SewingOperationLines",
                columns: new[] { "ProductId", "OperationId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SewingMaterialLines_Reference",
                schema: "catalog",
                table: "SewingMaterialLines",
                sql: "(\"Kind\" = 1 AND \"FabricId\" > 0 AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NULL AND \"Unit\" = 1) OR (\"Kind\" = 2 AND \"FabricId\" IS NULL AND \"AccessoryId\" > 0 AND \"CostCode\" IS NULL AND \"Unit\" = 2) OR (\"Kind\" = 3 AND \"FabricId\" IS NULL AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NOT NULL AND length(trim(\"CostCode\")) > 0 AND \"Unit\" = 3)");
        }
    }
}
