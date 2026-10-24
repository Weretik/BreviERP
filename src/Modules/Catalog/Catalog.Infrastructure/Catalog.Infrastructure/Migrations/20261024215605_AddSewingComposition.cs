using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CompositionVersion",
                schema: "catalog",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ModelCode",
                schema: "catalog",
                table: "Products",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SewingArticles",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SaleSku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingArticles", x => x.Id);
                    table.UniqueConstraint("AK_SewingArticles_ProductId_Id", x => new { x.ProductId, x.Id });
                    table.CheckConstraint("CK_SewingArticles_SortOrder_NonNegative", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_SewingArticles_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SewingOperationLines",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    OperationId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ManualMinutes = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingOperationLines", x => x.Id);
                    table.CheckConstraint("CK_SewingOperationLines_ManualMinutes_Positive", "\"ManualMinutes\" IS NULL OR \"ManualMinutes\" > 0");
                    table.CheckConstraint("CK_SewingOperationLines_OperationId_Positive", "\"OperationId\" > 0");
                    table.CheckConstraint("CK_SewingOperationLines_Quantity_Positive", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_SewingOperationLines_ReviewMetadata", "NOT \"IsReviewed\" OR (\"ReviewedBy\" IS NOT NULL AND length(trim(\"ReviewedBy\")) > 0 AND \"ReviewedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_SewingOperationLines_SharedOnly", "\"ArticleId\" IS NULL");
                    table.CheckConstraint("CK_SewingOperationLines_SortOrder_NonNegative", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_SewingOperationLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SewingMaterialLines",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    FabricId = table.Column<int>(type: "integer", nullable: true),
                    AccessoryId = table.Column<int>(type: "integer", nullable: true),
                    CostCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingMaterialLines", x => x.Id);
                    table.CheckConstraint("CK_SewingMaterialLines_Quantity_Positive", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_SewingMaterialLines_Reference", "(\"Kind\" = 1 AND \"FabricId\" > 0 AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NULL AND \"Unit\" = 1) OR (\"Kind\" = 2 AND \"FabricId\" IS NULL AND \"AccessoryId\" > 0 AND \"CostCode\" IS NULL AND \"Unit\" = 2) OR (\"Kind\" = 3 AND \"FabricId\" IS NULL AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NOT NULL AND length(trim(\"CostCode\")) > 0 AND \"Unit\" = 3)");
                    table.CheckConstraint("CK_SewingMaterialLines_ReviewMetadata", "NOT \"IsReviewed\" OR (\"ReviewedBy\" IS NOT NULL AND length(trim(\"ReviewedBy\")) > 0 AND \"ReviewedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_SewingMaterialLines_SortOrder_NonNegative", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_SewingMaterialLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SewingMaterialLines_SewingArticles_ProductId_ArticleId",
                        columns: x => new { x.ProductId, x.ArticleId },
                        principalSchema: "catalog",
                        principalTable: "SewingArticles",
                        principalColumns: new[] { "ProductId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ModelCode",
                schema: "catalog",
                table: "Products",
                column: "ModelCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingArticles_SaleSku",
                schema: "catalog",
                table: "SewingArticles",
                column: "SaleSku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingMaterialLines_ProductId_ArticleId",
                schema: "catalog",
                table: "SewingMaterialLines",
                columns: new[] { "ProductId", "ArticleId" });

            migrationBuilder.CreateIndex(
                name: "IX_SewingOperationLines_ProductId_OperationId",
                schema: "catalog",
                table: "SewingOperationLines",
                columns: new[] { "ProductId", "OperationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SewingMaterialLines",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingOperationLines",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingArticles",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_ModelCode",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CompositionVersion",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModelCode",
                schema: "catalog",
                table: "Products");
        }
    }
}
