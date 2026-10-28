using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreserveIssuedQuoteVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IssuedVersionId",
                schema: "crm",
                table: "Quotes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Id_IssuedVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "IssuedVersionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_QuoteVersions_Id_IssuedVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "IssuedVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_QuoteVersions_Id_IssuedVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_Id_IssuedVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "IssuedVersionId",
                schema: "crm",
                table: "Quotes");
        }
    }
}
