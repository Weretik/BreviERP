using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reference.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FabricColors",
                schema: "reference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricColors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricColors_Name",
                schema: "reference",
                table: "FabricColors",
                column: "Name",
                unique: true);

            // The sheet «Тканини», column «Загальний перелік кольорів» (client copy of 2026-11-11), in sheet order.
            migrationBuilder.InsertData(
                schema: "reference",
                table: "FabricColors",
                columns: ["Id", "Name", "IsActive", "SortOrder"],
                values: new object[,]
                {
                    { 1, "Чорний", true, 1 },
                    { 2, "Синій", true, 2 },
                    { 3, "Сірий", true, 3 },
                    { 4, "Світло-сірий", true, 4 },
                    { 5, "Темно-сірий", true, 5 },
                    { 6, "Зелений", true, 6 },
                    { 7, "Білий", true, 7 },
                    { 8, "Волошковий", true, 8 },
                    { 9, "Електрик", true, 9 },
                    { 10, "Коричневий", true, 10 },
                    { 11, "Червоний", true, 11 },
                    { 12, "Жовтий", true, 12 },
                    { 13, "Помаранчевий", true, 13 },
                    { 14, "Помаранчевий люмінесцентний", true, 14 },
                    { 15, "Блакитний", true, 15 },
                    { 16, "Бежевий", true, 16 },
                    { 17, "Темно-синій", true, 17 },
                    { 18, "Темно-синій кол.1076", true, 18 },
                    { 19, "Сірий кол.1128", true, 19 },
                    { 20, "Сірий кол.601", true, 20 },
                    { 21, "Зелений кол.1342", true, 21 },
                    { 22, "Волошковий кол.1223", true, 22 },
                    { 23, "Помаранчевий люмінесцентний кол.1207", true, 23 },
                    { 24, "Коричневий кол.1261", true, 24 },
                    { 25, "Бордовий", true, 25 },
                    { 26, "мал.Піксель", true, 26 },
                    { 27, "мал. Тайга", true, 27 },
                    { 28, "мал. Біла Береза", true, 28 },
                    { 29, "мал. Дубок", true, 29 },
                    { 30, "мал. Нато", true, 30 },
                    { 31, "мал. Місто", true, 31 },
                    { 32, "Асфальт", true, 32 },
                    { 33, "Лимонний", true, 33 },
                    { 34, "Салатовий", true, 34 },
                    { 35, "Світло-бежевий", true, 35 },
                    { 36, "Світло-синій", true, 36 },
                    { 37, "Салатовий люмінесцентний", true, 37 },
                    { 38, "Світло-жовтий", true, 38 },
                    { 39, "Жовтий люмінесцентний", true, 39 },
                    { 40, "Темно-зелений", true, 40 },
                    { 41, "Кораловий", true, 41 },
                    { 42, "Трав'яний", true, 42 },
                    { 43, "Світло-зелений", true, 43 },
                    { 44, "Лавандовий", true, 44 },
                    { 45, "Фуксія", true, 45 },
                    { 46, "Фіолетовий", true, 46 },
                    { 47, "Пильна троянда", true, 47 },
                    { 48, "Малиновий", true, 48 },
                    { 49, "Цегляний", true, 49 },
                    { 50, "Сіро-блакитний", true, 50 },
                    { 51, "Фіалковий", true, 51 },
                    { 52, "Бірюзовий", true, 52 },
                    { 53, "Мокрий асфальт", true, 53 },
                    { 54, "М'ятний", true, 54 },
                    { 55, "Ментоловий", true, 55 },
                    { 56, "Рожевий", true, 56 },
                    { 57, "Темно-коричневий", true, 57 },
                    { 58, "Хакі", true, 58 },
                    { 59, "Тайга", true, 59 },
                    { 60, "Льон полоска", true, 60 },
                    { 61, "Вафля", true, 61 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricColors",
                schema: "reference");
        }
    }
}
