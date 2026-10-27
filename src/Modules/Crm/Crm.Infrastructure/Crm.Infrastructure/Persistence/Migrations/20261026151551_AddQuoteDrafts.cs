using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculationBases",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    QuotePublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationBases", x => x.Id);
                    table.CheckConstraint("CK_CalculationBases_Expiry", "\"ExpiresAt\" > \"CreatedAt\"");
                });

            migrationBuilder.CreateTable(
                name: "QuoteMutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QuotePublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteMutationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuoteItemVariants",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    BasisToken = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceBasisJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItemVariants", x => x.Id);
                    table.CheckConstraint("CK_QuoteItemVariants_Kind", "\"Kind\" IN ('sewing','ppe','oneOffPpe')");
                });

            migrationBuilder.CreateTable(
                name: "QuoteVariantTiers",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VariantId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVariantTiers", x => x.Id);
                    table.CheckConstraint("CK_QuoteVariantTiers_Amount", "\"Amount\" IS NULL OR \"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_QuoteVariantTiers_QuoteItemVariants_VariantId",
                        column: x => x.VariantId,
                        principalSchema: "crm",
                        principalTable: "QuoteItemVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    ContactPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    Number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.CheckConstraint("CK_Quotes_Revision", "\"Revision\" > 0");
                    table.CheckConstraint("CK_Quotes_State", "\"State\" IN ('draft','ready','sent')");
                    table.ForeignKey(
                        name: "FK_Quotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "crm",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteVersions",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ParentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CustomerPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TaxBasis = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVersions", x => x.Id);
                    table.CheckConstraint("CK_QuoteVersions_Number", "\"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_QuoteVersions_QuoteVersions_ParentVersionId",
                        column: x => x.ParentVersionId,
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteVersions_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalSchema: "crm",
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteVersionItems",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PublicDescription = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    PublicComment = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    InternalNote = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Included = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteVersionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteVersionItems_QuoteVersions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "crm",
                        principalTable: "QuoteVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculationBases_ActorId_RequestId",
                schema: "crm",
                table: "CalculationBases",
                columns: new[] { "ActorId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculationBases_ExpiresAt",
                schema: "crm",
                table: "CalculationBases",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CalculationBases_PublicId",
                schema: "crm",
                table: "CalculationBases",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItemVariants_ItemId_SortOrder",
                schema: "crm",
                table: "QuoteItemVariants",
                columns: new[] { "ItemId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteMutationRequests_ActorId_Operation_RequestId",
                schema: "crm",
                table: "QuoteMutationRequests",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteMutationRequests_QuotePublicId",
                schema: "crm",
                table: "QuoteMutationRequests",
                column: "QuotePublicId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                column: "CurrentVersionId");

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
                unique: true,
                filter: "\"Number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_OwnerId_Archived_UpdatedAt_Id",
                schema: "crm",
                table: "Quotes",
                columns: new[] { "OwnerId", "Archived", "UpdatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_PublicId",
                schema: "crm",
                table: "Quotes",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVariantTiers_VariantId_Code",
                schema: "crm",
                table: "QuoteVariantTiers",
                columns: new[] { "VariantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersionItems_VersionId_SortOrder",
                schema: "crm",
                table: "QuoteVersionItems",
                columns: new[] { "VersionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_ParentVersionId",
                schema: "crm",
                table: "QuoteVersions",
                column: "ParentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_PublicId",
                schema: "crm",
                table: "QuoteVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteVersions_QuoteId_VersionNumber",
                schema: "crm",
                table: "QuoteVersions",
                columns: new[] { "QuoteId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteItemVariants_QuoteVersionItems_ItemId",
                schema: "crm",
                table: "QuoteItemVariants",
                column: "ItemId",
                principalSchema: "crm",
                principalTable: "QuoteVersionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_QuoteVersions_CurrentVersionId",
                schema: "crm",
                table: "Quotes",
                column: "CurrentVersionId",
                principalSchema: "crm",
                principalTable: "QuoteVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_QuoteVersions_CurrentVersionId",
                schema: "crm",
                table: "Quotes");

            migrationBuilder.DropTable(
                name: "CalculationBases",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteMutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteVariantTiers",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteItemVariants",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteVersionItems",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "QuoteVersions",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Quotes",
                schema: "crm");
        }
    }
}
