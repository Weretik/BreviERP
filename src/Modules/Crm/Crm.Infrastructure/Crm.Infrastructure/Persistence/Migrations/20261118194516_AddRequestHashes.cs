using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                schema: "crm",
                table: "SiteRequests",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                schema: "crm",
                table: "QuoteSendings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateRequestHash",
                schema: "crm",
                table: "Quotes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                schema: "crm",
                table: "LeadCalls",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestHash",
                schema: "crm",
                table: "SiteRequests");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                schema: "crm",
                table: "QuoteSendings");

            migrationBuilder.DropColumn(
                name: "CreateRequestHash",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                schema: "crm",
                table: "LeadCalls");
        }
    }
}
