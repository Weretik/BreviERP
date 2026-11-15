using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "accounting",
                table: "Transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "accounting",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_RequestId",
                schema: "accounting",
                table: "Transfers",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RequestId",
                schema: "accounting",
                table: "Transactions",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transfers_RequestId",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RequestId",
                schema: "accounting",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "accounting",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "accounting",
                table: "Transactions");
        }
    }
}
