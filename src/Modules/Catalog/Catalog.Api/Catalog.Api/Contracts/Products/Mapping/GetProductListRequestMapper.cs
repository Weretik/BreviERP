using Ardalis.Result;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Domain.Products.Enums;

namespace Catalog.Api.Contracts.Products;

public static class GetProductListRequestMapper
{
    public static Result<GetAdminProductsQuery> Map(GetProductListRequest request)
    {
        var validationErrors = new List<ValidationError>();
        ProductType? productType = null;

        if (request.Type is not null)
        {
            if (TryParseProductType(request.Type, out var parsedType))
                productType = parsedType;
            else
                validationErrors.Add(new ValidationError("type", "type must be either Sewing or Ppe."));
        }

        var isSortDirectionValid = request.SortDirection is "asc" or "desc";
        if (!isSortDirectionValid)
            validationErrors.Add(new ValidationError("sortDirection", "sortDirection must be either asc or desc."));

        return validationErrors.Count > 0
            ? Result.Invalid(validationErrors)
            : Result.Success(new GetAdminProductsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                productType,
                request.CategoryId,
                request.SortBy,
                request.SortDirection == "desc"));
    }

    private static bool TryParseProductType(string type, out ProductType productType)
    {
        productType = type switch
        {
            "Sewing" => ProductType.Sewing,
            "Ppe" => ProductType.Ppe,
            _ => default
        };
        return type is "Sewing" or "Ppe";
    }
}
