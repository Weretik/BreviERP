namespace Catalog.Api.Contracts.Products;

public sealed record GetProductListRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Type = null,
    int? CategoryId = null,
    string SortBy = "name",
    string SortDirection = "asc");
