using Ardalis.Specification.EntityFrameworkCore;
using Catalog.Application.Features.Media.GetList.Specifications;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public sealed class GetMediaFilesSpecTests
{
    [Test]
    public void List_query_orders_by_converted_media_file_id_in_sql()
    {
        using var db = new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql("Host=localhost;Database=translation_only;Username=test;Password=test")
                .Options);

        var query = SpecificationEvaluator.Default.GetQuery(
            db.MediaFiles.AsQueryable(),
            new GetMediaFilesSpec());

        var sql = query.ToQueryString();

        Assert.That(sql, Does.Contain("ORDER BY m.\"Id\" DESC"));
    }
}
