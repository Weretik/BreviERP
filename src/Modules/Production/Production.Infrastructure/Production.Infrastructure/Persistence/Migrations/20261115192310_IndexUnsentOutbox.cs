using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndexUnsentOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedAt_NextAttemptAt",
                schema: "production",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CreatedAt",
                schema: "production",
                table: "OutboxMessages",
                column: "CreatedAt",
                filter: "\"ProcessedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_CreatedAt",
                schema: "production",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt_NextAttemptAt",
                schema: "production",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAt", "NextAttemptAt" });
        }
    }
}
