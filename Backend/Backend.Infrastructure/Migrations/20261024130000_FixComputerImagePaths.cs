using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <summary>
    /// Изображения компьютеров переименованы в латиницу.
    ///
    /// Файлы с кириллическими именами при упаковке в zip на части систем превращались
    /// в «#U041e#U0444…» — сервер их не находил, и у компьютеров не было картинок.
    /// Кроме того, у «Бюджетный компьютер Lenovo» путь указывал на несуществующий файл.
    ///
    /// Миграция исправляет пути в уже заполненной базе (новая база сразу заполняется верными путями).
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261024130000_FixComputerImagePaths")]
    public partial class FixComputerImagePaths : Migration
    {
        private static readonly (string OldUrl, string NewUrl)[] Renames =
        {
            ("/images/computer/Игровой компьютер MSI.webp", "/images/computer/MSI Gaming PC.webp"),
            ("/images/computer/Офисный компьютер HP.webp", "/images/computer/HP Office PC.webp"),
            ("/images/computer/Домашний компьютер Dell.jpg", "/images/computer/Dell Home PC.jpg"),
            ("/images/computer/Игровой компьютер ASUS.webp", "/images/computer/ASUS Gaming PC.webp"),
            ("/images/computer/Бюджетный компьютер Lenovo.webp", "/images/computer/Lenovo PC.webp"),
            ("/images/computer/Игровой ПК MSI Infinite RS.webp", "/images/computer/MSI Infinite RS.webp"),
            ("/images/computer/Офисный ПК HP ProDesk 400.webp", "/images/computer/HP ProDesk 400.webp"),
            ("/images/computer/Домашний ПК Dell XPS 8960.webp", "/images/computer/Dell XPS 8960.webp"),
            ("/images/computer/Бюджетный ПК Acer Aspire TC.jpg", "/images/computer/Acer Aspire TC.jpg"),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (oldUrl, newUrl) in Renames)
                UpdateUrl(migrationBuilder, oldUrl, newUrl);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (oldUrl, newUrl) in Renames)
                UpdateUrl(migrationBuilder, newUrl, oldUrl);
        }

        // Обычный SQL: у миграции нет снимка модели (Designer), а UpdateData без модели
        // не может определить типы столбцов. Значения — константы из этого файла.
        private static void UpdateUrl(MigrationBuilder migrationBuilder, string from, string to)
        {
            migrationBuilder.Sql(
                $@"UPDATE ""ProductImages"" SET ""ImageUrl"" = '{Escape(to)}' WHERE ""ImageUrl"" = '{Escape(from)}';");
        }

        private static string Escape(string value) => value.Replace("'", "''");
    }
}
