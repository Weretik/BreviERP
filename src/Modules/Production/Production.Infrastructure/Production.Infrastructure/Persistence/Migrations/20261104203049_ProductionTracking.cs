using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                schema: "production",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStage",
                schema: "production",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Stages",
                schema: "production",
                table: "Items",
                type: "jsonb",
                nullable: false,
                defaultValue: "[1]");

            migrationBuilder.CreateTable(
                name: "OrderFiles",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Bytes = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderFiles_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "production",
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderFiles_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    TelegramEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: true),
                    TelegramThreadId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ShippedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "production",
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramDeliveries",
                schema: "production",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: true),
                    ThreadId = table.Column<int>(type: "integer", nullable: true),
                    MessageId = table.Column<long>(type: "bigint", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SentByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramDeliveries", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_TelegramDeliveries_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CurrentStage",
                schema: "production",
                table: "Items",
                column: "CurrentStage");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFiles_ItemId",
                schema: "production",
                table: "OrderFiles",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFiles_OrderId",
                schema: "production",
                table: "OrderFiles",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ItemId",
                schema: "production",
                table: "Shipments",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderId",
                schema: "production",
                table: "Shipments",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderFiles",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Shipments",
                schema: "production");

            migrationBuilder.DropTable(
                name: "TelegramDeliveries",
                schema: "production");

            migrationBuilder.DropIndex(
                name: "IX_Items_CurrentStage",
                schema: "production",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                schema: "production",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                schema: "production",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Stages",
                schema: "production",
                table: "Items");
        }
    }
}
