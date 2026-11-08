using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntroductionLetterAndPdfDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LetterBody",
                schema: "crm",
                table: "Quotes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LetterSubject",
                schema: "crm",
                table: "Quotes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            // Round 2, item 15: the default titles get their accent word, and the prices note under the models
            // starts with the sample's text. A title the Director changed stays as it is.
            migrationBuilder.Sql("""
                UPDATE crm."QuoteSettings"
                SET "Introduction" = "Introduction"
                    || CASE WHEN "Introduction"->>'summerTitle' = 'Спецодяг, створений для реальної роботи'
                        THEN '{"summerTitle": "Спецодяг, *створений* для реальної роботи"}'::jsonb ELSE '{}'::jsonb END
                    || CASE WHEN "Introduction"->>'winterTitle' = 'Зимовий спецодяг, створений для реальної роботи'
                        THEN '{"winterTitle": "Зимовий спецодяг, *створений* для реальної роботи"}'::jsonb ELSE '{}'::jsonb END
                    || CASE WHEN "Introduction"->'pricesNote' IS NULL
                        THEN '{"pricesNote": "Ціни за одиницю в обраній тканині, за замовлення від 41 шт., з ПДВ."}'::jsonb
                        ELSE '{}'::jsonb END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE crm."QuoteSettings"
                SET "Introduction" = jsonb_set(jsonb_set("Introduction" - 'pricesNote',
                    '{summerTitle}', to_jsonb(replace("Introduction"->>'summerTitle', '*', ''))),
                    '{winterTitle}', to_jsonb(replace("Introduction"->>'winterTitle', '*', '')))
                WHERE "Introduction"->>'summerTitle' IS NOT NULL AND "Introduction"->>'winterTitle' IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "LetterBody",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LetterSubject",
                schema: "crm",
                table: "Quotes");
        }
    }
}
