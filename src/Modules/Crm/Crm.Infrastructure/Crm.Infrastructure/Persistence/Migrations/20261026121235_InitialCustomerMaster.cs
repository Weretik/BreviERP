using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustomerMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TaxIdentity = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MergedIntoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.CheckConstraint("CK_Customers_Kind", "\"Kind\" IN ('organization','person')");
                    table.CheckConstraint("CK_Customers_Revision", "\"Revision\" > 0");
                    table.ForeignKey(
                        name: "FK_Customers_Customers_MergedIntoId",
                        column: x => x.MergedIntoId,
                        principalSchema: "crm",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MutationRequests",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerPublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerContacts",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContacts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "crm",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerExternalIdentities",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerExternalIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerExternalIdentities_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "crm",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactChannels",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Value = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactChannels", x => x.Id);
                    table.CheckConstraint("CK_ContactChannels_Kind", "\"Kind\" IN ('email','phone')");
                    table.ForeignKey(
                        name: "FK_ContactChannels_CustomerContacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "crm",
                        principalTable: "CustomerContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactChannels_ContactId_Kind_NormalizedValue",
                schema: "crm",
                table: "ContactChannels",
                columns: new[] { "ContactId", "Kind", "NormalizedValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactChannels_NormalizedValue",
                schema: "crm",
                table: "ContactChannels",
                column: "NormalizedValue");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_PublicId",
                schema: "crm",
                table: "CustomerContacts",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CustomerContacts_ActivePrimary",
                schema: "crm",
                table: "CustomerContacts",
                column: "CustomerId",
                unique: true,
                filter: "\"IsPrimary\" AND \"Active\"");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerExternalIdentities_CustomerId",
                schema: "crm",
                table: "CustomerExternalIdentities",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerExternalIdentities_Source_Type_ExternalId",
                schema: "crm",
                table: "CustomerExternalIdentities",
                columns: new[] { "Source", "Type", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_MergedIntoId",
                schema: "crm",
                table: "Customers",
                column: "MergedIntoId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NormalizedName",
                schema: "crm",
                table: "Customers",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_OwnerId_Active_UpdatedAt",
                schema: "crm",
                table: "Customers",
                columns: new[] { "OwnerId", "Active", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PublicId",
                schema: "crm",
                table: "Customers",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TaxIdentity",
                schema: "crm",
                table: "Customers",
                column: "TaxIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_MutationRequests_ActorId_Operation_RequestId",
                schema: "crm",
                table: "MutationRequests",
                columns: new[] { "ActorId", "Operation", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MutationRequests_CustomerPublicId",
                schema: "crm",
                table: "MutationRequests",
                column: "CustomerPublicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactChannels",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomerExternalIdentities",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "MutationRequests",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "CustomerContacts",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "crm");
        }
    }
}
