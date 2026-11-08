using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Purchasing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UnlinkedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UnlinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "purchasing",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MessageId_Id",
                schema: "purchasing",
                table: "Payments",
                columns: new[] { "MessageId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MessageId_TransactionId",
                schema: "purchasing",
                table: "Payments",
                columns: new[] { "MessageId", "TransactionId" },
                unique: true,
                filter: "\"UnlinkedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                schema: "purchasing",
                table: "Payments",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments",
                schema: "purchasing");
        }
    }
}
