using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductionOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: true),
                    QuoteNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    ClientName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ManagerUserId = table.Column<int>(type: "integer", nullable: true),
                    ManagerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PriceTier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreateRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CopiedFromOrderId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    QuoteItemId = table.Column<long>(type: "bigint", nullable: true),
                    QuoteOptionId = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    ModelCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ModelName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FabricId = table.Column<int>(type: "integer", nullable: true),
                    FabricCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FabricName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SewingRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    ReportUnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MainColor = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SizesText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TechnicalChanges = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PlannedFabricMeters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    AdditionalColors = table.Column<string>(type: "jsonb", nullable: false),
                    Applications = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderChanges",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: true),
                    ItemName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Field = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Before = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    After = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderChanges_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ModelCode",
                schema: "production",
                table: "Items",
                column: "ModelCode");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OrderId_Position",
                schema: "production",
                table: "Items",
                columns: new[] { "OrderId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderChanges_OrderId",
                schema: "production",
                table: "OrderChanges",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreateRequestId",
                schema: "production",
                table: "Orders",
                column: "CreateRequestId",
                unique: true,
                filter: "\"CreateRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LegacyId",
                schema: "production",
                table: "Orders",
                column: "LegacyId",
                unique: true,
                filter: "\"LegacyId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Number",
                schema: "production",
                table: "Orders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_QuoteId",
                schema: "production",
                table: "Orders",
                column: "QuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items",
                schema: "production");

            migrationBuilder.DropTable(
                name: "OrderChanges",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "production");
        }
    }
}
