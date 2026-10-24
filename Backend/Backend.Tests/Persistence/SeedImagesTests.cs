using Backend.Infrastructure.Persistence;
using FluentAssertions;

namespace Backend.Tests.Persistence;

/// <summary>
/// Каждое изображение из тестовых данных должно существовать в wwwroot и иметь имя из ASCII-символов.
/// Кириллические имена файлов при упаковке в zip на части систем превращались в «#U041e#U0444…»,
/// и сервер отдавал 404 — у всех компьютеров пропали картинки.
/// </summary>
public class SeedImagesTests
{
    [Fact]
    public void Seed_AllImages_ExistInWwwroot_AndHaveAsciiNames()
    {
        using var db = TestDbContextFactory.CreateInMemory();
        DbInitializer.Seed(db);

        var urls = db.ProductImages.Select(i => i.ImageUrl).ToList()
            .Concat(db.Categories.Select(c => c.ImageUrl).ToList().Where(u => u != null).Select(u => u!))
            .ToList();

        urls.Should().NotBeEmpty();

        var wwwroot = FindWwwroot();
        var problems = new List<string>();

        foreach (var url in urls)
        {
            if (url.Any(ch => ch > 127))
                problems.Add($"не ASCII: {url}");

            var path = Path.Combine(wwwroot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                problems.Add($"нет файла: {url}");
        }

        problems.Should().BeEmpty();
    }

    private static string FindWwwroot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "wwwroot");
            if (Directory.Exists(Path.Combine(candidate, "images")))
                return candidate;

            candidate = Path.Combine(dir.FullName, "Backend", "wwwroot");
            if (Directory.Exists(Path.Combine(candidate, "images")))
                return candidate;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Папка Backend/wwwroot не найдена");
    }
}
