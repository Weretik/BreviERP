using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbroideryArchiveLegacyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacyId",
                schema: "crm",
                table: "EmbroideryCalculations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmbroideryCalculations_LegacyId",
                schema: "crm",
                table: "EmbroideryCalculations",
                column: "LegacyId",
                unique: true,
                filter: "\"LegacyId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmbroideryCalculations_LegacyId",
                schema: "crm",
                table: "EmbroideryCalculations");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                schema: "crm",
                table: "EmbroideryCalculations");
        }
    }
}
