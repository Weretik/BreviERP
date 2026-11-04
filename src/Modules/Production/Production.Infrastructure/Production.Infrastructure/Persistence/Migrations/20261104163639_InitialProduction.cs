using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Production.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "production");

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Position = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricNorms",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModelCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OneColor = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    TwoColorsMain = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    TwoColorsAdditional = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    ThreeColorsMain = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    ThreeColorsAdditional1 = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    ThreeColorsAdditional2 = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricNorms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NameKey",
                schema: "production",
                table: "Employees",
                column: "NameKey",
                unique: true,
                filter: "\"IsActive\"");

            migrationBuilder.CreateIndex(
                name: "IX_FabricNorms_ModelCode",
                schema: "production",
                table: "FabricNorms",
                column: "ModelCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees",
                schema: "production");

            migrationBuilder.DropTable(
                name: "FabricNorms",
                schema: "production");
        }
    }
}
