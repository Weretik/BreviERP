using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceQuoteVersionOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_QuoteVersions_CurrentVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteVersions_QuoteVersions_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions");

            migrationBuilder.DropIndex(
                name: "IX_QuoteVersions_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CurrentVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_QuoteVersions_QuoteId_Id",
                schema: "crm",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_QuoteId_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "ParentVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Id_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "CurrentVersionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_QuoteVersions_Id_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "Id", "CurrentVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteVersions_QuoteVersions_QuoteId_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "ParentVersionId" },
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumns: new[] { "QuoteId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_QuoteVersions_Id_CurrentVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteVersions_QuoteVersions_QuoteId_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_QuoteVersions_QuoteId_Id",
                schema: "crm",
                table: "QuoteVersions");

            migrationBuilder.DropIndex(
                name: "IX_QuoteVersions_QuoteId_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_Id_CurrentVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions",
                column: "ParentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                column: "CurrentVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_QuoteVersions_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                column: "CurrentVersionId",
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteVersions_QuoteVersions_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions",
                column: "ParentVersionId",
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
