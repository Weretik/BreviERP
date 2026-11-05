using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkshopArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveRecords",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SourceRow = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    DateText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Client = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Product = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Quantity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Fabric = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SearchText = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Fields = table.Column<string>(type: "jsonb", nullable: false),
                    Links = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_Kind_Date",
                schema: "production",
                table: "ArchiveRecords",
                columns: new[] { "Kind", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_SourceKey",
                schema: "production",
                table: "ArchiveRecords",
                column: "SourceKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveRecords",
                schema: "production");
        }
    }
}
