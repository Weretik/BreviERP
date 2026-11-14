using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreservePricingOperationPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "numeric(24,8)",
                precision: 24,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                schema: "reference",
                table: "GarmentPartOperations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(24,8)",
                oldPrecision: 24,
                oldScale: 8);
        }
    }
}
