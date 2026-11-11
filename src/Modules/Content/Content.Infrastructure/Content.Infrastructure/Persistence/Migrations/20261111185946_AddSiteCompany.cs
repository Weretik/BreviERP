using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteCompany",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    MapQuery = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Schedule = table.Column<string>(type: "text", nullable: false),
                    Managers = table.Column<string>(type: "text", nullable: false),
                    BankDetails = table.Column<string>(type: "text", nullable: false),
                    Socials = table.Column<string>(type: "text", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteCompany", x => x.Id);
                    table.CheckConstraint("CK_SiteCompany_Single", "\"Id\" = 1");
                });

            // The facts the site showed before (CNT-009 R-02), so the page opens filled.
            migrationBuilder.InsertData(
                schema: "content",
                table: "SiteCompany",
                columns: new[] { "Id", "Phone", "Email", "City", "Street", "MapQuery", "Schedule", "Managers", "BankDetails", "Socials", "Revision", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { 1, "050 469 86 75", "pp.brevi@gmail.com", "{\"uk\": \"Полтава\", \"ru\": \"Полтава\"}", "{\"uk\": \"вул. Кагамлика, 3А\", \"ru\": \"ул. Кагамлыка, 3А\"}", "Полтава, вулиця Кагамлика, 3А", "{\"days\": [1, 2, 3, 4, 5], \"opens\": \"09:00\", \"closes\": \"18:00\"}", "[{\"name\": {\"uk\": \"Анастасія Берегових\", \"ru\": \"Анастасия Береговых\"}, \"photoUrl\": \"/assets/contacts/anastasiia-berehovykh.webp\", \"phones\": [\"050 309 06 60\", \"067 446 75 46\"], \"email\": \"brevianastasia@gmail.com\"}, {\"name\": {\"uk\": \"Вадим Осауленко\", \"ru\": \"Вадим Осауленко\"}, \"photoUrl\": \"/assets/contacts/vadym-osaulenko.webp\", \"phones\": [\"050 346 75 46\"], \"email\": \"brevi.pl05@gmail.com\"}]", "{\"recipient\": {\"uk\": \"Приватне підприємство «Бреві»\", \"ru\": \"Частное предприятие «Бреви»\"}, \"edrpou\": \"35521636\", \"iban\": \"UA63 305299 00000 26004041206862\", \"bank\": {\"uk\": \"АТ КБ «ПриватБанк»\", \"ru\": \"АО КБ «ПриватБанк»\"}}", "{\"instagram\": null, \"youtube\": null, \"facebook\": null}", 1, new DateTimeOffset(2026, 11, 11, 0, 0, 0, TimeSpan.Zero), "Дані сайту" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteCompany",
                schema: "content");
        }
    }
}
