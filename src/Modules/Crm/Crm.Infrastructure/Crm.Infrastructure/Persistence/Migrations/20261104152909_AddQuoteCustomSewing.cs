using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteCustomSewing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.AddColumn<decimal>(
                name: "FabricMeters",
                schema: "crm",
                table: "QuoteItemOptions",
                type: "numeric(12,4)",
                precision: 12,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuoteItemLines",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    ManualMinutes = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItemLines", x => x.Id);
                    table.CheckConstraint("CK_QuoteItemLines_ManualMinutes", "\"ManualMinutes\" IS NULL OR (\"Kind\" = 'Operation' AND \"ManualMinutes\" > 0)");
                    table.CheckConstraint("CK_QuoteItemLines_Position", "\"Position\" >= 0");
                    table.CheckConstraint("CK_QuoteItemLines_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_QuoteItemLines_QuoteItems_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "crm",
                        principalTable: "QuoteItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" IN ('OneOffPpe', 'CustomSewing') AND \"ProductId\" IS NULL) OR (\"Kind\" NOT IN ('OneOffPpe', 'CustomSewing') AND \"ProductId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemLines_ItemId_Position",
                schema: "crm",
                table: "QuoteItemLines",
                columns: new[] { "ItemId", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteItemLines",
                schema: "crm");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "FabricMeters",
                schema: "crm",
                table: "QuoteItemOptions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuoteItems_Product",
                schema: "crm",
                table: "QuoteItems",
                sql: "(\"Kind\" = 'OneOffPpe' AND \"ProductId\" IS NULL) OR (\"Kind\" <> 'OneOffPpe' AND \"ProductId\" IS NOT NULL)");
        }
    }
}
