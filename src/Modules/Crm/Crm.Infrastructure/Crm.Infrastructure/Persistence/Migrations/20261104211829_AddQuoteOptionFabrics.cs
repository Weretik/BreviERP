using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteOptionFabrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuoteItemOptionFabrics",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    FabricId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Meters = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItemOptionFabrics", x => x.Id);
                    table.CheckConstraint("CK_QuoteItemOptionFabrics_Fabric", "\"FabricId\" > 0");
                    table.CheckConstraint("CK_QuoteItemOptionFabrics_Meters", "\"Meters\" > 0");
                    table.CheckConstraint("CK_QuoteItemOptionFabrics_Position", "\"Position\" >= 0");
                    table.ForeignKey(
                        name: "FK_QuoteItemOptionFabrics_QuoteItemOptions_OptionId",
                        column: x => x.OptionId,
                        principalSchema: "crm",
                        principalTable: "QuoteItemOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemOptionFabrics_OptionId_Position",
                schema: "crm",
                table: "QuoteItemOptionFabrics",
                columns: new[] { "OptionId", "Position" });

            // A saved «Виріб з нуля» variant had one fabric on the row itself (R-26).
            migrationBuilder.Sql("""
                INSERT INTO crm."QuoteItemOptionFabrics" ("OptionId", "Position", "FabricId", "Name", "Meters")
                SELECT o."Id", 0, o."FabricId", LEFT(o."Title", 200), o."FabricMeters"
                FROM crm."QuoteItemOptions" o
                JOIN crm."QuoteItems" i ON i."Id" = o."ItemId"
                WHERE i."Kind" = 'CustomSewing' AND o."FabricId" > 0 AND o."FabricMeters" > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteItemOptionFabrics",
                schema: "crm");
        }
    }
}
