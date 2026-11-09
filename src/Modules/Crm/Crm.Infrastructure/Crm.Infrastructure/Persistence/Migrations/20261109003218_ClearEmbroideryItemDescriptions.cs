using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClearEmbroideryItemDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The client does not need quantity, stitches and hoop in the КП: the generated text is removed from
            // embroidery items; a description the manager wrote stays.
            migrationBuilder.Sql("""
                UPDATE crm."QuoteItems"
                SET "Description" = NULL
                WHERE "Kind" = 'Embroidery'
                  AND "Description" ~ '^[0-9\s\u00A0\u202F]+ шт · [0-9\s\u00A0\u202F]+ стібків на 1 шт · п’яльця .*\.$';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The removed texts are not restored.
        }
    }
}
