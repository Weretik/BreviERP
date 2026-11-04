using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteIntroductions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntroLayout",
                schema: "crm",
                table: "Quotes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LetterName",
                schema: "crm",
                table: "Quotes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                schema: "crm",
                table: "Quotes",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuoteIntroTemplates",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Layout = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LetterName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Pages = table.Column<string>(type: "jsonb", nullable: false),
                    AuthorUserId = table.Column<int>(type: "integer", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteIntroTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIntroTemplates_NameKey",
                schema: "crm",
                table: "QuoteIntroTemplates",
                column: "NameKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteIntroTemplates",
                schema: "crm");

            migrationBuilder.DropColumn(
                name: "IntroLayout",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LetterName",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                schema: "crm",
                table: "Quotes");
        }
    }
}
