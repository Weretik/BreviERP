using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Reminders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reminders");

            migrationBuilder.CreateTable(
                name: "Chats",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    TelegramTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsBotMember = table.Column<bool>(type: "boolean", nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonWorkingDays",
                schema: "reminders",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonWorkingDays", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Organization = table.Column<int>(type: "integer", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: true),
                    ActiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LeadDays = table.Column<int>(type: "integer", nullable: false),
                    Periodicity = table.Column<int>(type: "integer", nullable: false),
                    DueDay = table.Column<int>(type: "integer", nullable: false),
                    DueRule = table.Column<int>(type: "integer", nullable: false),
                    DueMonthOffset = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "reminders",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Occurrences",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    Periodicity = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SnoozedUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    ClosedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClosedByTelegramUserId = table.Column<long>(type: "bigint", nullable: true),
                    ClosedByAdminUserId = table.Column<int>(type: "integer", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PostponedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PostponedByTelegramUserId = table.Column<long>(type: "bigint", nullable: true),
                    PostponedByAdminUserId = table.Column<int>(type: "integer", nullable: true),
                    PostponedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Occurrences_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "reminders",
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OccurrenceActions",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActorTelegramUserId = table.Column<long>(type: "bigint", nullable: true),
                    ActorAdminUserId = table.Column<int>(type: "integer", nullable: true),
                    At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SnoozedUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    OccurrenceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccurrenceActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccurrenceActions_Occurrences_OccurrenceId",
                        column: x => x.OccurrenceId,
                        principalSchema: "reminders",
                        principalTable: "Occurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentMessages",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OccurrenceId = table.Column<long>(type: "bigint", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: false),
                    TelegramMessageId = table.Column<long>(type: "bigint", nullable: false),
                    SentOn = table.Column<DateOnly>(type: "date", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HasButtons = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentMessages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "reminders",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SentMessages_Occurrences_OccurrenceId",
                        column: x => x.OccurrenceId,
                        principalSchema: "reminders",
                        principalTable: "Occurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_TelegramChatId",
                schema: "reminders",
                table: "Chats",
                column: "TelegramChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OccurrenceActions_OccurrenceId",
                schema: "reminders",
                table: "OccurrenceActions",
                column: "OccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_Status_DueDate",
                schema: "reminders",
                table: "Occurrences",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_TaskId_PeriodStart",
                schema: "reminders",
                table: "Occurrences",
                columns: new[] { "TaskId", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SentMessages_ChatId",
                schema: "reminders",
                table: "SentMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_SentMessages_OccurrenceId_HasButtons",
                schema: "reminders",
                table: "SentMessages",
                columns: new[] { "OccurrenceId", "HasButtons" });

            migrationBuilder.CreateIndex(
                name: "IX_SentMessages_OccurrenceId_SentOn",
                schema: "reminders",
                table: "SentMessages",
                columns: new[] { "OccurrenceId", "SentOn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ChatId",
                schema: "reminders",
                table: "Tasks",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NonWorkingDays",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "OccurrenceActions",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "SentMessages",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "Occurrences",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "Chats",
                schema: "reminders");
        }
    }
}
