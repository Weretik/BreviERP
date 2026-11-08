using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                schema: "crm",
                table: "ManagerTasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeadCallResults",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RetryDays = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadCallResults", x => x.Id);
                    table.CheckConstraint("CK_LeadCallResults_RetryDays", "(\"Outcome\" = 'Retry' AND \"RetryDays\" BETWEEN 1 AND 60) OR (\"Outcome\" <> 'Retry' AND \"RetryDays\" IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "LeadCategories",
                schema: "crm",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadCategories", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    AddedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Category = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Region = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Scale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Signal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SignalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FactOrPlan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SignalUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VolumeReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Offer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Website = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Contacts = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WhomToCall = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TenderCheck = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CallReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Reliability = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SheetNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InSheet = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerUserId = table.Column<int>(type: "integer", nullable: true),
                    OwnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TakenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Stage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastResultName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    LastCallAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextContactAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CallCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.CheckConstraint("CK_Leads_Owner", "(\"OwnerUserId\" IS NULL) = (\"OwnerName\" IS NULL)");
                    table.CheckConstraint("CK_Leads_SourceId", "\"SourceId\" > 0");
                    table.CheckConstraint("CK_Leads_Version", "\"Version\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "LeadSyncStates",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Added = table.Column<int>(type: "integer", nullable: false),
                    Updated = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadSyncStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadCalls",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeadId = table.Column<long>(type: "bigint", nullable: false),
                    ByUserId = table.Column<int>(type: "integer", nullable: false),
                    ByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResultId = table.Column<long>(type: "bigint", nullable: false),
                    ResultName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NextContactAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadCalls_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "crm",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "crm",
                table: "LeadCallResults",
                columns: new[] { "Id", "IsActive", "Name", "Outcome", "Position", "RetryDays" },
                values: new object[,]
                {
                    { 1L, true, "Не дозвонились", "Retry", 10, 1 },
                    { 2L, true, "Не відповідають", "Retry", 20, 2 },
                    { 3L, true, "Передзвонити", "Callback", 30, null },
                    { 4L, true, "Вийшли на відповідального", "Talking", 40, null },
                    { 5L, true, "Надіслали пропозицію", "Offer", 50, null },
                    { 6L, true, "Перенесено в CRM", "Client", 60, null },
                    { 7L, true, "Відмова", "Closed", 70, null },
                    { 8L, true, "Нецільове підприємство", "Closed", 80, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTasks_LeadId_Status",
                schema: "crm",
                table: "ManagerTasks",
                columns: new[] { "LeadId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadCalls_At",
                schema: "crm",
                table: "LeadCalls",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_LeadCalls_ByUserId_At",
                schema: "crm",
                table: "LeadCalls",
                columns: new[] { "ByUserId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_LeadCalls_LeadId",
                schema: "crm",
                table: "LeadCalls",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Category",
                schema: "crm",
                table: "Leads",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerUserId_Stage_NextContactAt",
                schema: "crm",
                table: "Leads",
                columns: new[] { "OwnerUserId", "Stage", "NextContactAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_SourceId",
                schema: "crm",
                table: "Leads",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Stage_OwnerUserId",
                schema: "crm",
                table: "Leads",
                columns: new[] { "Stage", "OwnerUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadCallResults",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "LeadCalls",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "LeadCategories",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "LeadSyncStates",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Leads",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_ManagerTasks_LeadId_Status",
                schema: "crm",
                table: "ManagerTasks");

            migrationBuilder.DropColumn(
                name: "LeadId",
                schema: "crm",
                table: "ManagerTasks");
        }
    }
}
