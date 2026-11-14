using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <summary>
    /// Catalog stops mapping MediaFiles: the photo library moved to the Media module, whose InitialMedia migration copies the rows
    /// with the same ids. The old catalog."MediaFiles" table stays in the database untouched until the copy is checked in production;
    /// a later migration drops it.
    /// </summary>
    public partial class MoveMediaFilesToMediaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: the table is kept as a fallback copy.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to restore: Up does not change the database.
        }
    }
}
