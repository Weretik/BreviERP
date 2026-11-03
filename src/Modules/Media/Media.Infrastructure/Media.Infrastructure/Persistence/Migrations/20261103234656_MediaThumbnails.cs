using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MediaThumbnails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ImageProcessedAt",
                schema: "media",
                table: "Files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailStorageKey",
                schema: "media",
                table: "Files",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                schema: "media",
                table: "Files",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_ImageProcessedAt",
                schema: "media",
                table: "Files",
                column: "ImageProcessedAt",
                filter: "\"ImageProcessedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_ImageProcessedAt",
                schema: "media",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ImageProcessedAt",
                schema: "media",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ThumbnailStorageKey",
                schema: "media",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                schema: "media",
                table: "Files");
        }
    }
}
