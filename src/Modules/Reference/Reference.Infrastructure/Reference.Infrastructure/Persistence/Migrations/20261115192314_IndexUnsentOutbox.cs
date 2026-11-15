using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndexUnsentOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedAt_NextAttemptAt",
                schema: "reference",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CreatedAt",
                schema: "reference",
                table: "OutboxMessages",
                column: "CreatedAt",
                filter: "\"ProcessedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_CreatedAt",
                schema: "reference",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt_NextAttemptAt",
                schema: "reference",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAt", "NextAttemptAt" });
        }
    }
}
