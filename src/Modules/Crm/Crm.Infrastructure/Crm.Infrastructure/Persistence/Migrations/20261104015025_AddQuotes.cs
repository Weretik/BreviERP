using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quotes",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumberYear = table.Column<int>(type: "integer", nullable: false),
                    NumberMonth = table.Column<int>(type: "integer", nullable: false),
                    NumberSequence = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InternalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClientComment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ManagerNote = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AuthorUserId = table.Column<int>(type: "integer", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResponsibleUserId = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.CheckConstraint("CK_Quotes_NumberMonth", "\"NumberMonth\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_Quotes_NumberSequence", "\"NumberSequence\" > 0");
                    table.CheckConstraint("CK_Quotes_Version", "\"Version\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "QuoteItems",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SiteUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Season = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SupplierUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItems", x => x.Id);
                    table.CheckConstraint("CK_QuoteItems_Position", "\"Position\" >= 0");
                    table.CheckConstraint("CK_QuoteItems_Product", "(\"Kind\" = 'OneOffPpe' AND \"ProductId\" IS NULL) OR (\"Kind\" <> 'OneOffPpe' AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_QuoteItems_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuoteItemOptions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    FabricId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false),
                    ManualReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ManualByUserId = table.Column<int>(type: "integer", nullable: true),
                    ManualByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ManualAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PricesTakenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LargeManual = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    LargeShown = table.Column<bool>(type: "boolean", nullable: false),
                    LargeSource = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    RetailManual = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    RetailShown = table.Column<bool>(type: "boolean", nullable: false),
                    RetailSource = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    WholesaleManual = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    WholesaleShown = table.Column<bool>(type: "boolean", nullable: false),
                    WholesaleSource = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItemOptions", x => x.Id);
                    table.CheckConstraint("CK_QuoteItemOptions_Large", "(\"LargeSource\" IS NULL OR \"LargeSource\" >= 0) AND (\"LargeManual\" IS NULL OR \"LargeManual\" >= 0)");
                    table.CheckConstraint("CK_QuoteItemOptions_Position", "\"Position\" >= 0");
                    table.CheckConstraint("CK_QuoteItemOptions_Retail", "(\"RetailSource\" IS NULL OR \"RetailSource\" >= 0) AND (\"RetailManual\" IS NULL OR \"RetailManual\" >= 0)");
                    table.CheckConstraint("CK_QuoteItemOptions_Wholesale", "(\"WholesaleSource\" IS NULL OR \"WholesaleSource\" >= 0) AND (\"WholesaleManual\" IS NULL OR \"WholesaleManual\" >= 0)");
                    table.ForeignKey(
                        name: "FK_QuoteItemOptions_QuoteItems_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "crm",
                        principalTable: "QuoteItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemOptions_ArticleId",
                schema: "crm",
                table: "QuoteItemOptions",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemOptions_ItemId_Position",
                schema: "crm",
                table: "QuoteItemOptions",
                columns: new[] { "ItemId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_ProductId",
                schema: "crm",
                table: "QuoteItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_QuoteId_Position",
                schema: "crm",
                table: "QuoteItems",
                columns: new[] { "QuoteId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CustomerId",
                schema: "crm",
                table: "Quotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Number",
                schema: "crm",
                table: "Quotes",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_NumberYear_NumberMonth_NumberSequence",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "NumberYear", "NumberMonth", "NumberSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ResponsibleUserId_UpdatedAt",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "ResponsibleUserId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_UpdatedAt",
                schema: "crm",
                table: "Quotes",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteItemOptions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteItems",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Quotes",
                schema: "crm");
        }
    }
}
