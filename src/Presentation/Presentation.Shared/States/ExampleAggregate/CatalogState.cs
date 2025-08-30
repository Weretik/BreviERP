namespace Presentation.Shared.States.ExampleAggregate;

[FeatureState]
public sealed record CatalogState(
    CatalogParams Params,
    bool IsLoading = false,
    string? Error = null,
    PaginatedList<ProductDto>? PageList = null )
{
    private CatalogState() : this(new CatalogParams(), false, null, null) { }
}
