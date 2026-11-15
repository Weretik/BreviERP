using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreateRequestIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "crm",
                table: "SiteRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "crm",
                table: "QuoteSendings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateRequestId",
                schema: "crm",
                table: "Quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "crm",
                table: "LeadCalls",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteRequests_RequestId",
                schema: "crm",
                table: "SiteRequests",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteSendings_RequestId",
                schema: "crm",
                table: "QuoteSendings",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CreateRequestId",
                schema: "crm",
                table: "Quotes",
                column: "CreateRequestId",
                unique: true,
                filter: "\"CreateRequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeadCalls_RequestId",
                schema: "crm",
                table: "LeadCalls",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SiteRequests_RequestId",
                schema: "crm",
                table: "SiteRequests");

            migrationBuilder.DropIndex(
                name: "IX_QuoteSendings_RequestId",
                schema: "crm",
                table: "QuoteSendings");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CreateRequestId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_LeadCalls_RequestId",
                schema: "crm",
                table: "LeadCalls");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "crm",
                table: "SiteRequests");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "crm",
                table: "QuoteSendings");

            migrationBuilder.DropColumn(
                name: "CreateRequestId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "crm",
                table: "LeadCalls");
        }
    }
}
