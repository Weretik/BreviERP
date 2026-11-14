namespace Catalog.Application.Features.Product.Queries.GetProductMediaUsages;

/// <summary>Products whose photos use the given Media library files; the Media module asks before deleting a photo.</summary>
public sealed record GetProductMediaUsagesQuery(IReadOnlyCollection<int> MediaFileIds)
    : IQuery<Result<IReadOnlyList<ProductMediaUsage>>>;

public sealed record ProductMediaUsage(int MediaFileId, int ProductId, string ProductName);
