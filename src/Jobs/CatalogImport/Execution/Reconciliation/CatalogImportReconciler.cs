using CatalogImport.Source;
using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Persistence;
using CatalogImport.Planning;
using Microsoft.EntityFrameworkCore;

namespace CatalogImport.Execution;

public static class CatalogImportReconciler
{
    public static async Task VerifyAsync(CatalogDbContext db, CatalogImportPlan plan, CatalogImportReport report,
        CancellationToken cancellationToken, IReadOnlyDictionary<string, long> mapping)
    {
        db.ChangeTracker.Clear();
        var products = await db.Products.AsNoTracking().Include(x => x.Articles).Include(x => x.MaterialLines)
            .Include(x => x.OperationLines).Include(x => x.CatalogProfile).AsSplitQuery().ToListAsync(cancellationToken);
        if (products.Count != report.Products
            || products.Sum(x => x.Articles.Count) != report.Articles
            || products.Sum(x => x.MaterialLines.Count) != report.Materials
            || products.Sum(x => x.OperationLines.Count) != report.Operations)
            throw new InvalidDataException("Persisted catalogue counts do not reconcile.");

        foreach (var product in products.Where(x => x.ModelCode != null))
        {
            var modelCode = product.ModelCode!;
            var sourceMaterials = plan.Materials.Where(row => row.Text(0) == modelCode).ToArray();
            var sourceOperations = plan.Operations.Where(row => row.Text(1) == modelCode).ToArray();
            var sourceArticles = plan.Articles.Where(row => CatalogSourceIdentity.ModelCode(row) == modelCode)
                .Select(row => row.Text(0)).ToHashSet();
            if (!product.Articles.Select(x => x.SaleSku).ToHashSet().SetEquals(sourceArticles))
                throw new InvalidDataException($"Article identities differ for {modelCode}.");
            if (product.MaterialLines.Count != sourceMaterials.Length || product.OperationLines.Count != sourceOperations.Length)
                throw new InvalidDataException($"Composition counts differ for {modelCode}.");
            foreach (var line in product.MaterialLines)
            {
                var row = sourceMaterials[line.SortOrder];
                var actualSku = line.ArticleId.HasValue
                    ? product.Articles.Single(a => a.Id == line.ArticleId).SaleSku : string.Empty;
                var sourceCode = row.Text(6);
                var expectedUnit = row.Text(9) == "м" ? "Metre" : row.Text(9) == "послуга" ? "Service" : "Piece";
                var referenceMatches = row.Text(9) == "послуга" ? line.CostCode == sourceCode
                    : sourceCode.StartsWith("FAB-", StringComparison.Ordinal)
                        ? line.FabricId?.Value == mapping["Fabric:" + sourceCode]
                        : line.AccessoryId?.Value == mapping["Accessory:" + sourceCode];
                if (line.Quantity != row.Decimal(8) || actualSku != row.Text(1)
                    || !referenceMatches || line.Unit.ToString() != expectedUnit || line.IsRequired != row.Flag(12))
                    throw new InvalidDataException($"Material quantity or article differs at {row.Key}.");
            }
            foreach (var line in product.OperationLines)
            {
                var row = sourceOperations[line.SortOrder];
                if (line.Quantity != row.Decimal(7) || line.ManualMinutes != row.Decimal(8)
                    || line.OperationId.Value != mapping["Operation:" + CatalogSourceIdentity.OperationKey(row)])
                    throw new InvalidDataException($"Operation quantity/time differs at {row.Key}.");
            }
        }
        foreach (var product in products.Where(x => x.CatalogProfile?.SaleSku != null))
        {
            var row = plan.Ppe.Single(row => row.Text(0) == product.CatalogProfile!.SaleSku);
            if (product.CatalogProfile!.SupplierPrice != row.Decimal(7)
                || product.CatalogProfile.SupplierId?.Value != mapping["SupplierName:" + row.Text(5)]
                || product.CatalogProfile.SupplierSku != Empty(row.Text(14))
                || product.CatalogProfile.Unit != Empty(row.Text(3)))
                throw new InvalidDataException($"Supplier price differs at {row.Key}.");
        }
    }

    private static string? Empty(string value) => value.Length == 0 ? null : value;
}

