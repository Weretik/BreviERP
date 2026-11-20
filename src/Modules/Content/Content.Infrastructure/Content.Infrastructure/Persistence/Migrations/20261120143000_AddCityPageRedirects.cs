using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// The 26 city pages of the old site lead to the new city pages «Пошиття» and «Робочий одяг» (brevi-web city pages,
    /// 2026-11-20), so staff do not type them in «Контент → Переадресації». A redirect staff added or changed by hand
    /// for the same old address is kept; one made by the old-site transfer is pointed at the new page.
    /// </summary>
    public partial class AddCityPageRedirects : Migration
    {
        /// <summary>Who the rows are written by, as the other site data of the migrations.</summary>
        public const string Author = "Дані сайту";

        /// <summary>Old address (normalized, no language) → page of the new site, and the note staff see.</summary>
        public static readonly IReadOnlyList<CityPageRedirect> Rows =
        [
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-kyyevi", "/uk/sewing/kyiv", "Пошиття, Київ"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-harkovi", "/uk/sewing/kharkiv", "Пошиття, Харків"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-dnipri", "/uk/sewing/dnipro", "Пошиття, Дніпро"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-v-odesi", "/uk/sewing/odesa", "Пошиття, Одеса"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-zaporizhzhi", "/uk/sewing/zaporizhzhia", "Пошиття, Запоріжжя"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-kryvyj-rig", "/uk/sewing/kryvyi-rih", "Пошиття, Кривий Ріг"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-mykolayevi", "/uk/sewing/mykolaiv", "Пошиття, Миколаїв"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-vinnyci", "/uk/sewing/vinnytsia", "Пошиття, Вінниця"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-poltavi", "/uk/sewing/poltava", "Пошиття, Полтава"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-chernigovi", "/uk/sewing/chernihiv", "Пошиття, Чернігів"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-zhytomyri", "/uk/sewing/zhytomyr", "Пошиття, Житомир"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-u-sumah", "/uk/sewing/sumy", "Пошиття, Суми"),
            new("/poshyttya-specodyagu-na-zamovlennya-ta-optovyj-prodazh-bila-cerkva", "/uk/sewing/bila-tserkva", "Пошиття, Біла Церква"),
            new("/robochyj-odyag/specodyag-kyyiv", "/uk/workwear/kyiv", "Робочий одяг, Київ"),
            new("/robochyj-odyag/specodyag-harkiv", "/uk/workwear/kharkiv", "Робочий одяг, Харків"),
            new("/robochyj-odyag/specodyag-dnipro", "/uk/workwear/dnipro", "Робочий одяг, Дніпро"),
            new("/robochyj-odyag/specodyag-odesa", "/uk/workwear/odesa", "Робочий одяг, Одеса"),
            new("/robochyj-odyag/specodyag-zaporizhzhya", "/uk/workwear/zaporizhzhia", "Робочий одяг, Запоріжжя"),
            new("/robochyj-odyag/specodyag-kryvyj-rig", "/uk/workwear/kryvyi-rih", "Робочий одяг, Кривий Ріг"),
            new("/robochyj-odyag/specodyag-mykolayiv", "/uk/workwear/mykolaiv", "Робочий одяг, Миколаїв"),
            new("/robochyj-odyag/specodyag-vinnycya", "/uk/workwear/vinnytsia", "Робочий одяг, Вінниця"),
            new("/robochyj-odyag/specodyag-poltava", "/uk/workwear/poltava", "Робочий одяг, Полтава"),
            new("/robochyj-odyag/specodyag-chernigiv", "/uk/workwear/chernihiv", "Робочий одяг, Чернігів"),
            new("/robochyj-odyag/specodyag-zhytomyr", "/uk/workwear/zhytomyr", "Робочий одяг, Житомир"),
            new("/robochyj-odyag/specodyag-sumy", "/uk/workwear/sumy", "Робочий одяг, Суми"),
            new("/robochyj-odyag/specodyag-bila-cerkva", "/uk/workwear/bila-tserkva", "Робочий одяг, Біла Церква"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var values = string.Join(",\n", Rows.Select(row =>
                $"('{row.From}', '{row.To}', 2, '{row.Note}', TIMESTAMPTZ '2026-11-20 00:00:00+00', '{Author}')"));
            // Source 2 is SiteRedirectSource.OldSite; 1 (Manual) rows are never touched.
            migrationBuilder.Sql($@"INSERT INTO content.""SiteRedirects"" (""FromPath"", ""ToPath"", ""Source"", ""Note"", ""UpdatedAtUtc"", ""UpdatedBy"")
VALUES
{values}
ON CONFLICT (""FromPath"") DO UPDATE SET
    ""ToPath"" = EXCLUDED.""ToPath"", ""Note"" = EXCLUDED.""Note"", ""Source"" = EXCLUDED.""Source"",
    ""UpdatedAtUtc"" = EXCLUDED.""UpdatedAtUtc"", ""UpdatedBy"" = EXCLUDED.""UpdatedBy""
WHERE content.""SiteRedirects"".""Source"" <> 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var from = string.Join(", ", Rows.Select(row => $"'{row.From}'"));
            migrationBuilder.Sql($@"DELETE FROM content.""SiteRedirects""
WHERE ""FromPath"" IN ({from}) AND ""Source"" = 2 AND ""UpdatedBy"" = '{Author}';");
        }
    }

    /// <summary>One row of <see cref="AddCityPageRedirects.Rows"/>.</summary>
    public sealed record CityPageRedirect(string From, string To, string Note);
}
