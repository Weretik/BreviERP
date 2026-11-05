using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForecastQueues",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrderIds = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Work",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OverPlan = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VoidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VoidedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VoidReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LegacyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Work_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "production",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Work_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "production",
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Work_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "production",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Work_EmployeeId",
                schema: "production",
                table: "Work",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Work_ItemId",
                schema: "production",
                table: "Work",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Work_LegacyId",
                schema: "production",
                table: "Work",
                column: "LegacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Work_OrderId",
                schema: "production",
                table: "Work",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Work_WorkDate",
                schema: "production",
                table: "Work",
                column: "WorkDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastQueues",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Work",
                schema: "production");
        }
    }
}
