using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Purchasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "purchasing",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                schema: "purchasing",
                table: "Messages",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewChanges",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reviewer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AfterNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AfterSupplierId = table.Column<int>(type: "integer", nullable: true),
                    AfterVersion = table.Column<int>(type: "integer", nullable: false),
                    BeforeNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BeforeSupplierId = table.Column<int>(type: "integer", nullable: true),
                    BeforeVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewChanges_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "purchasing",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewChanges_MessageId_Id",
                schema: "purchasing",
                table: "ReviewChanges",
                columns: new[] { "MessageId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewChanges",
                schema: "purchasing");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "purchasing",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                schema: "purchasing",
                table: "Messages");
        }
    }
}
