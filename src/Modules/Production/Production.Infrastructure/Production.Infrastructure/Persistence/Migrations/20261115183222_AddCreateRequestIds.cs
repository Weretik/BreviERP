using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreateRequestIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                schema: "production",
                table: "Work",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Work_RequestId",
                schema: "production",
                table: "Work",
                column: "RequestId",
                unique: true,
                filter: "\"RequestId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Work_RequestId",
                schema: "production",
                table: "Work");

            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "production",
                table: "Work");
        }
    }
}
