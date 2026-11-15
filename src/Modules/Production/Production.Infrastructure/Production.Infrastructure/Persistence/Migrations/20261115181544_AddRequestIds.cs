using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "production",
                table: "PayrollPayments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "production",
                table: "PayrollAdjustments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_RequestId",
                schema: "production",
                table: "PayrollPayments",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_RequestId",
                schema: "production",
                table: "PayrollAdjustments",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollPayments_RequestId",
                schema: "production",
                table: "PayrollPayments");

            migrationBuilder.DropIndex(
                name: "IX_PayrollAdjustments_RequestId",
                schema: "production",
                table: "PayrollAdjustments");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "production",
                table: "PayrollPayments");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "production",
                table: "PayrollAdjustments");
        }
    }
}
