using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.GetList.Specifications;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.GetList;

public sealed class GetProductOverviewQueryHandler(ICatalogReadRepository<ProductEntity> repository)
    : IQueryHandler<GetProductOverviewQuery, Result<IReadOnlyList<ProductOverviewRow>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductOverviewRow>>> Handle(
        GetProductOverviewQuery query, CancellationToken cancellationToken)
    {
        var rows = await repository.ListAsync(new ProductOverviewSpec(query.Type), cancellationToken);
        return Result.Success<IReadOnlyList<ProductOverviewRow>>(rows
            .Select(x => new ProductOverviewRow(
                x.Id, ProductOverviewTypes.Name(x.Type), x.Type == ProductType.Ppe ? x.SaleSku : x.ModelCode, x.Name,
                x.SectionId, x.Season, x.IsActive, x.IsVerified, x.VerifiedOn, x.CurrentSiteUrl, x.QuoteDescription,
                x.UpdatedAt ?? x.CreatedAt))
            .OrderBy(x => x.Code is null)
            .ThenBy(x => x.Code, CodeComparer.Instance)
            .ThenBy(x => x.Name, StringComparer.CurrentCulture)
            .ToList());
    }

    /// <summary>Orders "MOD-2" before "MOD-10" and PPE article "9" before "17".</summary>
    private sealed class CodeComparer : IComparer<string?>
    {
        public static readonly CodeComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            var (xPrefix, xNumber) = Split(x);
            var (yPrefix, yNumber) = Split(y);
            var byPrefix = string.CompareOrdinal(xPrefix, yPrefix);
            if (byPrefix != 0) return byPrefix;
            var byNumber = xNumber.CompareTo(yNumber);
            return byNumber != 0 ? byNumber : string.CompareOrdinal(x, y);
        }

        private static (string Prefix, long Number) Split(string? value)
        {
            value ??= string.Empty;
            var end = value.Length;
            while (end > 0 && char.IsAsciiDigit(value[end - 1])) end--;
            return long.TryParse(value.AsSpan(end), out var number) && value.Length - end <= 18
                ? (value[..end], number)
                : (value, long.MaxValue);
        }
    }
}
