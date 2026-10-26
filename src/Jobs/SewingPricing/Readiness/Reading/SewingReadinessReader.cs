using System.Data;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.History.Publications.Reading;
using Catalog.Infrastructure.Pricing.Reading;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Pricing.Reading;
using SewingPricing.Jobs.Reading;

namespace SewingPricing.Jobs.Readiness;

internal static class SewingReadinessReader
{
    public static async Task<IReadOnlyList<PricingReadinessItem>> ReadAsync(
        string connectionString, IReadOnlyList<WorkbookRow> rows, DateTimeOffset at, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection).Options);
        await using var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options);
        await catalog.Database.UseTransactionAsync(transaction, cancellationToken);
        await reference.Database.UseTransactionAsync(transaction, cancellationToken);
        var skus = rows.Select(x => x.Text(0)).ToArray();
        var articles = await catalog.Set<SewingArticle>().AsNoTracking().Where(x => skus.Contains(x.SaleSku))
            .Select(x => new { Id = x.Id.Value, ProductId = x.ProductId.Value, x.SaleSku }).ToListAsync(cancellationToken);
        var bySku = articles.ToDictionary(x => x.SaleSku, StringComparer.Ordinal);
        var publicationReader = new SewingPublishedPriceReader(catalog);
        var publications = new Dictionary<long, Guid>();
        foreach (var chunk in articles.Select(x => x.ProductId).Distinct().Chunk(100))
            foreach (var product in await publicationReader.ReadAsync(chunk, cancellationToken))
                foreach (var publication in product.Articles)
                    publications.Add(publication.ArticleId, publication.PublicationId);
        var evaluator = new CalculateSewingArticleQueryHandler(new SewingPricingInputReader(catalog), new SewingPricingSettingsReader(catalog),
            new JobPricingReferenceReader(new ReferencePricingVersionReader(reference)));
        var items = new List<PricingReadinessItem>();
        foreach (var row in rows)
        {
            var issues = PricingReadinessSourceCheck.Inspect(row);
            if (!bySku.TryGetValue(row.Text(0), out var article))
            {
                items.Add(new(row.Text(0), "Sewing", null, null, false, [new("MissingArticle", row.Text(0))], issues));
                continue;
            }
            var result = await evaluator.Handle(new(article.ProductId, article.Id, at), cancellationToken);
            if (!result.IsSuccess)
                throw new InvalidDataException($"Cannot resolve imported Article {row.Text(0)}: {result.Status}.");
            var preview = result.Value;
            items.Add(new(row.Text(0), "Sewing", article.ProductId, article.Id,
                preview.Calculation.Prices is not null && preview.Calculation.Blockers.Count == 0,
                preview.Calculation.Blockers.Select(x => new PricingReadinessBlocker(x.Code, x.InputKey)).ToArray(), issues)
            {
                SettingsVersionId = preview.SettingsVersionId, ReferenceVersionIds = preview.ReferenceVersionIds,
                PublicationId = publications.TryGetValue(article.Id, out var id) ? id : null
            });
        }
        return items;
    }
}
