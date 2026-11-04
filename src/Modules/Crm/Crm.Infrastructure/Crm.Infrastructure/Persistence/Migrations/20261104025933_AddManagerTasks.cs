using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagerTasks",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AssigneeUserId = table.Column<int>(type: "integer", nullable: false),
                    AssigneeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorUserId = table.Column<int>(type: "integer", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsUrgent = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerPublicId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerTasks", x => x.Id);
                    table.CheckConstraint("CK_ManagerTasks_Completed", "(\"Status\" = 'Done' AND \"CompletedAt\" IS NOT NULL) OR (\"Status\" <> 'Done' AND \"CompletedAt\" IS NULL)");
                    table.CheckConstraint("CK_ManagerTasks_Version", "\"Version\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "ManagerTaskEvents",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ByUserId = table.Column<int>(type: "integer", nullable: false),
                    ByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerTaskEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagerTaskEvents_ManagerTasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "crm",
                        principalTable: "ManagerTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagerTaskSteps",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerTaskSteps", x => x.Id);
                    table.CheckConstraint("CK_ManagerTaskSteps_Position", "\"Position\" >= 0");
                    table.ForeignKey(
                        name: "FK_ManagerTaskSteps_ManagerTasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "crm",
                        principalTable: "ManagerTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTaskEvents_TaskId_At",
                schema: "crm",
                table: "ManagerTaskEvents",
                columns: new[] { "TaskId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTasks_AssigneeUserId_Status_DueAt",
                schema: "crm",
                table: "ManagerTasks",
                columns: new[] { "AssigneeUserId", "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTasks_CompletedAt",
                schema: "crm",
                table: "ManagerTasks",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTasks_Status_DueAt",
                schema: "crm",
                table: "ManagerTasks",
                columns: new[] { "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerTaskSteps_TaskId_Position",
                schema: "crm",
                table: "ManagerTaskSteps",
                columns: new[] { "TaskId", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagerTaskEvents",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "ManagerTaskSteps",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "ManagerTasks",
                schema: "crm");
        }
    }
}
