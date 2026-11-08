using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmbroideryPriceCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "crm",
                table: "EmbroideryCalculations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            // «Опт» is removed; «Стандарт» and «Терміново» keep their percents as «Категорія 1» and «Категорія 2».
            migrationBuilder.Sql("""
                UPDATE crm."EmbroiderySettings"
                SET "Content" = jsonb_set("Content", '{markups}', jsonb_build_array(
                    jsonb_build_object('name', 'Категорія 1', 'percent', "Content"->'markups'->1->'percent'),
                    jsonb_build_object('name', 'Категорія 2', 'percent', "Content"->'markups'->2->'percent')))
                WHERE jsonb_array_length("Content"->'markups') = 3;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The old «Опт» comes back with its default 60 %.
            migrationBuilder.Sql("""
                UPDATE crm."EmbroiderySettings"
                SET "Content" = jsonb_set("Content", '{markups}', jsonb_build_array(
                    jsonb_build_object('name', 'Опт', 'percent', 60),
                    jsonb_build_object('name', 'Стандарт', 'percent', "Content"->'markups'->0->'percent'),
                    jsonb_build_object('name', 'Терміново', 'percent', "Content"->'markups'->1->'percent')))
                WHERE jsonb_array_length("Content"->'markups') = 2;
                """);

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "crm",
                table: "EmbroideryCalculations");
        }
    }
}
