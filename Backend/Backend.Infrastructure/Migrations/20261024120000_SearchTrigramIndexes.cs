using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <summary>
    /// Триграммные GIN-индексы для поиска товаров.
    ///
    /// Поиск выполняется через <c>ILIKE '%запрос%'</c> по названию и описанию. Обычный B-tree индекс
    /// такой шаблон использовать не может, и каждый поиск читал всю таблицу. Индексы pg_trgm
    /// позволяют PostgreSQL искать подстроку по индексу (BitmapOr по двум индексам).
    ///
    /// Модель EF не меняется (индексы только в SQL), поэтому снимок модели не обновляется.
    /// Если у пользователя БД нет прав на CREATE EXTENSION (часть облачных СУБД), миграция
    /// не падает: индексы просто не создаются, поиск работает как раньше.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261024120000_SearchTrigramIndexes")]
    public partial class SearchTrigramIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    CREATE EXTENSION IF NOT EXISTS pg_trgm;
EXCEPTION
    WHEN insufficient_privilege THEN
        RAISE NOTICE 'pg_trgm недоступно: триграммные индексы поиска не будут созданы';
END
$$;");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm') THEN
        CREATE INDEX IF NOT EXISTS ""IX_Products_Name_Trgm""
            ON ""Products"" USING gin (""Name"" gin_trgm_ops);
        CREATE INDEX IF NOT EXISTS ""IX_Products_Description_Trgm""
            ON ""Products"" USING gin (""Description"" gin_trgm_ops);
    END IF;
END
$$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Products_Description_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Products_Name_Trgm"";");
        }
    }
}
