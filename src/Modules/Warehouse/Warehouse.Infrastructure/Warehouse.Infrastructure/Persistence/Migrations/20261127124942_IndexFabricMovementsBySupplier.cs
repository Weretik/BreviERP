using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndexFabricMovementsBySupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FabricMovements_SupplierId_OperationDate",
                schema: "warehouse",
                table: "FabricMovements",
                columns: new[] { "SupplierId", "OperationDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FabricMovements_SupplierId_OperationDate",
                schema: "warehouse",
                table: "FabricMovements");
        }
    }
}
