using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                schema: "production",
                table: "Work",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                schema: "production",
                table: "PayrollPayments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                schema: "production",
                table: "PayrollAdjustments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateRequestHash",
                schema: "production",
                table: "Orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestHash",
                schema: "production",
                table: "Work");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                schema: "production",
                table: "PayrollPayments");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                schema: "production",
                table: "PayrollAdjustments");

            migrationBuilder.DropColumn(
                name: "CreateRequestHash",
                schema: "production",
                table: "Orders");
        }
    }
}
