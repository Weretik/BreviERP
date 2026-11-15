using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reminders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrenceVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "reminders",
                table: "Occurrences",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "reminders",
                table: "Occurrences");
        }
    }
}
