using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferencePricingInputVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingInputVersions",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FabricId = table.Column<int>(type: "integer", nullable: true),
                    AccessoryId = table.Column<int>(type: "integer", nullable: true),
                    OperationId = table.Column<int>(type: "integer", nullable: true),
                    OtherCostCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Value = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceDate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingInputVersions", x => x.Id);
                    table.CheckConstraint("CK_PricingInputVersions_NonNegative", "\"Value\" IS NULL OR \"Value\" >= 0");
                    table.CheckConstraint("CK_PricingInputVersions_OneReference", "num_nonnulls(\"FabricId\", \"AccessoryId\", \"OperationId\", \"OtherCostCode\") = 1");
                    table.ForeignKey(
                        name: "FK_PricingInputVersions_FabricsReference_FabricId",
                        column: x => x.FabricId,
                        principalSchema: "reference",
                        principalTable: "FabricsReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PricingInputVersions_GarmentAccessoriesReference_AccessoryId",
                        column: x => x.AccessoryId,
                        principalSchema: "reference",
                        principalTable: "GarmentAccessoriesReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PricingInputVersions_GarmentPartOperations_OperationId",
                        column: x => x.OperationId,
                        principalSchema: "reference",
                        principalTable: "GarmentPartOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingInputVersions_AccessoryId_EffectiveFrom",
                schema: "reference",
                table: "PricingInputVersions",
                columns: new[] { "AccessoryId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingInputVersions_FabricId_EffectiveFrom",
                schema: "reference",
                table: "PricingInputVersions",
                columns: new[] { "FabricId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingInputVersions_OperationId_EffectiveFrom",
                schema: "reference",
                table: "PricingInputVersions",
                columns: new[] { "OperationId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingInputVersions_OtherCostCode_EffectiveFrom",
                schema: "reference",
                table: "PricingInputVersions",
                columns: new[] { "OtherCostCode", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingInputVersions_SourceCode_Revision",
                schema: "reference",
                table: "PricingInputVersions",
                columns: new[] { "SourceCode", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingInputVersions_SourceHash_SourceCode",
                schema: "reference",
                table: "PricingInputVersions",
                columns: new[] { "SourceHash", "SourceCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingInputVersions",
                schema: "reference");
        }
    }
}
