namespace Reference.Infrastructure.Seeders.ProductCategories.Rows;

public sealed record ProductCategorySeedRow(
    int Id,
    string Name,
    string RuName,
    string Slug,
    int? ParentId = null,
    int SortOrder = 0,
    bool IsActive = true);
