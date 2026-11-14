using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingPricingSettingsVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SewingPricingSettingsVersions",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    SourceHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceJson = table.Column<string>(type: "jsonb", nullable: false),
                    FormulaVersion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    WorkDays = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    ShiftMinutes = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    SeamstressCount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    AssemblyRate = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    CuttingRate = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    MasterRate = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    ManagerRate = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    BonusRate = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    MonthlyOverhead = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    RetailMultiplier = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    WholesaleMultiplier = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    LargeWholesaleMultiplier = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingPricingSettingsVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SewingPricingSettingsVersions_EffectiveFrom",
                schema: "catalog",
                table: "SewingPricingSettingsVersions",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_SewingPricingSettingsVersions_Revision",
                schema: "catalog",
                table: "SewingPricingSettingsVersions",
                column: "Revision",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingPricingSettingsVersions_SourceHash",
                schema: "catalog",
                table: "SewingPricingSettingsVersions",
                column: "SourceHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SewingPricingSettingsVersions",
                schema: "catalog");
        }
    }
}
