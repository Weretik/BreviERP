using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MediaFileSourceKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceKey",
                schema: "media",
                table: "Files",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_SourceKey",
                schema: "media",
                table: "Files",
                column: "SourceKey",
                unique: true,
                filter: "\"SourceKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_SourceKey",
                schema: "media",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "SourceKey",
                schema: "media",
                table: "Files");
        }
    }
}
