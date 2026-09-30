using Ardalis.Specification.EntityFrameworkCore;
using Catalog.Application.Features.Media.Shared.Specifications;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public sealed class MediaFileByIdSpecTests
{
    [Test]
    public void Query_compares_converted_media_file_id_in_sql()
    {
        using var db = new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only;Username=test;Password=test")
                .Options);

        var query = SpecificationEvaluator.Default.GetQuery(
            db.MediaFiles.AsQueryable(),
            new MediaFileByIdSpec(42));

        var sql = query.ToQueryString();

        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain("-- @mediaFileId='42'"));
            Assert.That(sql, Does.Contain("WHERE m.\"Id\" = @mediaFileId"));
        });
    }
}
