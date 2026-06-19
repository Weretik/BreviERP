using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Seeders.ProductCategories.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.ProductCategories;

public sealed class ProductCategorySeeder(
    ReferenceDbContext db,
    ILogger<ProductCategorySeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "ProductCategories", "Data", "product_category.json");
        if (await db.ProductCategories.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped ProductCategory seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<ProductCategorySeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<ProductCategorySeedRow>();

        var rowsById = rows.ToDictionary(x => x.Id);
        var pathById = new Dictionary<int, string>();

        foreach (var row in rows.OrderBy(x => x.ParentId.HasValue).ThenBy(x => x.SortOrder).ThenBy(x => x.Id))
        {
            var parentPath = ResolveParentPath(row, rowsById, pathById);

            var entity = ProductCategory.Create(
                ProductCategoryId.From(row.Id),
                row.Name.Trim(),
                row.RuName.Trim(),
                row.Slug.Trim(),
                row.ParentId.HasValue ? ProductCategoryId.From(row.ParentId.Value) : null,
                parentPath,
                row.SortOrder,
                row.IsActive);

            pathById[row.Id] = entity.Path;

            await db.ProductCategories.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded ProductCategory. Added: {AddedCount}", rows.Count);
    }

    private static string? ResolveParentPath(
        ProductCategorySeedRow row,
        IReadOnlyDictionary<int, ProductCategorySeedRow> rowsById,
        IDictionary<int, string> pathById)
    {
        if (!row.ParentId.HasValue)
            return null;

        if (pathById.TryGetValue(row.ParentId.Value, out var existingPath))
            return existingPath;

        if (!rowsById.TryGetValue(row.ParentId.Value, out var parentRow))
            throw new InvalidOperationException($"Product category seed parent {row.ParentId.Value} was not found.");

        var parentPath = ResolveParentPath(parentRow, rowsById, pathById);
        var parentEntity = ProductCategory.Create(
            ProductCategoryId.From(parentRow.Id),
            parentRow.Name.Trim(),
            parentRow.RuName.Trim(),
            parentRow.Slug.Trim(),
            parentRow.ParentId.HasValue ? ProductCategoryId.From(parentRow.ParentId.Value) : null,
            parentPath,
            parentRow.SortOrder,
            parentRow.IsActive);

        pathById[parentRow.Id] = parentEntity.Path;
        return parentEntity.Path;
    }
}
