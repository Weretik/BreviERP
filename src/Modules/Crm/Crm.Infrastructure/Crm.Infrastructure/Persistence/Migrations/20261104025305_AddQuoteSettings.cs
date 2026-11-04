using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuoteSettings",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Company = table.Column<string>(type: "jsonb", nullable: false),
                    Email = table.Column<string>(type: "jsonb", nullable: false),
                    Pdf = table.Column<string>(type: "jsonb", nullable: false),
                    Introduction = table.Column<string>(type: "jsonb", nullable: false),
                    LetterVariants = table.Column<string>(type: "jsonb", nullable: false),
                    StaffContacts = table.Column<string>(type: "jsonb", nullable: false),
                    UpdatedByName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteSettings", x => x.Id);
                    table.CheckConstraint("CK_QuoteSettings_Singleton", "\"Id\" = 1");
                    table.CheckConstraint("CK_QuoteSettings_Version", "\"Version\" > 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteSettings",
                schema: "crm");
        }
    }
}
