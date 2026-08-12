using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDetailsAndContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "catalog",
                table: "Products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionRu",
                schema: "catalog",
                table: "Products",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionUk",
                schema: "catalog",
                table: "Products",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "catalog",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "catalog",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PpeProductDetails",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    RetailAdditionalReferenceId = table.Column<int>(type: "integer", nullable: true),
                    RetailCustomPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    RetailPercentSource = table.Column<string>(type: "text", nullable: false),
                    WholesaleAdditionalReferenceId = table.Column<int>(type: "integer", nullable: true),
                    WholesaleCustomPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    WholesalePercentSource = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeProductDetails", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_PpeProductDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCharacteristicTables",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    TitleUk = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleRu = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCharacteristicTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCharacteristicTables_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductInformationBlocks",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    TitleUk = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleRu = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TextUk = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TextRu = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInformationBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductInformationBlocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SewingProductDetails",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    MetersPerProduct = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingProductDetails", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_SewingProductDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCharacteristicRows",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelUk = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelRu = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValueUk = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ValueRu = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCharacteristicRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCharacteristicRows_ProductCharacteristicTables_Table~",
                        column: x => x.TableId,
                        principalSchema: "catalog",
                        principalTable: "ProductCharacteristicTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SewingProductAccessories",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    GarmentAccessoryId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingProductAccessories", x => new { x.ProductId, x.GarmentAccessoryId });
                    table.ForeignKey(
                        name: "FK_SewingProductAccessories_SewingProductDetails_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "SewingProductDetails",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SewingProductFabrics",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    FabricId = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingProductFabrics", x => new { x.ProductId, x.FabricId });
                    table.ForeignKey(
                        name: "FK_SewingProductFabrics_SewingProductDetails_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "SewingProductDetails",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SewingProductOperations",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    GarmentPartOperationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingProductOperations", x => new { x.ProductId, x.GarmentPartOperationId });
                    table.ForeignKey(
                        name: "FK_SewingProductOperations_SewingProductDetails_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "SewingProductDetails",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                schema: "catalog",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_RuName",
                schema: "catalog",
                table: "Products",
                column: "RuName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                schema: "catalog",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCharacteristicRows_TableId",
                schema: "catalog",
                table: "ProductCharacteristicRows",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCharacteristicTables_ProductId",
                schema: "catalog",
                table: "ProductCharacteristicTables",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInformationBlocks_ProductId",
                schema: "catalog",
                table: "ProductInformationBlocks",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpeProductDetails",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "ProductCharacteristicRows",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "ProductInformationBlocks",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingProductAccessories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingProductFabrics",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingProductOperations",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "ProductCharacteristicTables",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SewingProductDetails",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_RuName",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionRu",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionUk",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "catalog",
                table: "Products");
        }
    }
}
