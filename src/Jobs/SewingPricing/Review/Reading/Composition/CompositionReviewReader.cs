using Catalog.Infrastructure.DataBase;
using CatalogImport.Persistence;
using CatalogImport.Planning;
using Microsoft.EntityFrameworkCore;

namespace SewingPricing.Jobs.Review;

internal static class CompositionReviewReader
{
    public static async Task<IReadOnlyList<PricingReviewSubject>> ReadAsync(CatalogDbContext db, CatalogImportPlan plan,
        IReadOnlyDictionary<string, long> mappings, CancellationToken cancellationToken)
    {
        var references = new ReferenceImportResult
        {
            Fabrics = ReferenceIds("Fabric"), Accessories = ReferenceIds("Accessory"), Operations = ReferenceIds("Operation")
        };
        Dictionary<string, int> ReferenceIds(string dataset) => mappings.Where(x => x.Key.StartsWith(dataset + ":", StringComparison.Ordinal))
            .ToDictionary(x => x.Key[(dataset.Length + 1)..], x => checked((int)x.Value), StringComparer.Ordinal);
        var products = await db.Products.AsNoTracking().Include(x => x.Articles).Include(x => x.MaterialLines)
            .Include(x => x.OperationLines).AsSplitQuery().ToListAsync(cancellationToken);
        var subjects = new List<PricingReviewSubject>();
        foreach (var model in plan.Models)
        {
            var product = products.SingleOrDefault(x => x.ModelCode == model.Text(0));
            var materials = plan.Materials.Where(x => x.Text(0) == model.Text(0)).ToArray();
            var operations = plan.Operations.Where(x => x.Text(1) == model.Text(0)).ToArray();
            subjects.Add(new(model.Text(0), "CompositionCount", product?.CompositionVersion.ToString(),
                PricingReviewComparison.Fields(new { Materials = materials.Length, Operations = operations.Length },
                    product is null ? null : new { Materials = product.MaterialLines.Count, Operations = product.OperationLines.Count })));
            if (product is null) continue;
            var articles = product.Articles.ToDictionary(x => x.SaleSku, StringComparer.Ordinal);
            for (var index = 0; index < materials.Length; index++)
            {
                var row = materials[index];
                var expected = CatalogMaterialLineMapper.Create(row, index, product, articles, references);
                var actual = product.MaterialLines.SingleOrDefault(x => x.Id.Value == mappings.GetValueOrDefault("MaterialLine:" + row.Key))
                    ?? product.MaterialLines.SingleOrDefault(x => x.ArticleId == expected.ArticleId && x.Kind == expected.Kind
                        && x.ReferenceKey == expected.ReferenceKey && x.SortOrder == expected.SortOrder);
                subjects.Add(new(row.Key, "MaterialLine", $"{product.Id.Value}:{product.CompositionVersion}:{actual?.Id.Value}",
                    MaterialLineReviewComparer.Compare(expected, actual))
                { IsReviewed = actual?.IsReviewed });
            }
            for (var index = 0; index < operations.Length; index++)
            {
                var row = operations[index];
                var expected = CatalogOperationLineMapper.Create(row, index, product.Id, references);
                var actual = product.OperationLines.SingleOrDefault(x => x.Id.Value == mappings.GetValueOrDefault("OperationLine:" + row.Key))
                    ?? product.OperationLines.SingleOrDefault(x => x.OperationId == expected.OperationId && x.SortOrder == expected.SortOrder);
                subjects.Add(new(row.Key, "OperationLine", $"{product.Id.Value}:{product.CompositionVersion}:{actual?.Id.Value}",
                    OperationLineReviewComparer.Compare(expected, actual))
                { IsReviewed = actual?.IsReviewed });
            }
        }
        return subjects;
    }

}
