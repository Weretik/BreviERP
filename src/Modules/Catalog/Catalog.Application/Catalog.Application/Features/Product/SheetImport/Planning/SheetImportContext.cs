using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>Everything the planner compares the sheet with; read before planning, nothing is read during it.</summary>
/// <param name="Products">All stored products with profile, articles and composition.</param>
/// <param name="CalculatedProductIds">Products with saved sewing calculations (the delete is restricted).</param>
/// <param name="CalculatedArticleIds">Articles with saved sewing calculations (the delete is restricted).</param>
/// <param name="KnownRuleCodes">PPE rule codes of the sheet that exist in Catalog.</param>
/// <param name="CurrentOffers">The latest PPE offer version by product ID.</param>
public sealed record SheetImportContext(
    IReadOnlyList<ProductEntity> Products,
    IReadOnlySet<int> CalculatedProductIds,
    IReadOnlySet<long> CalculatedArticleIds,
    ProductReferenceCodes References,
    IReadOnlySet<string> KnownRuleCodes,
    IReadOnlyDictionary<int, PpeOfferVersion> CurrentOffers,
    IProductSlugGenerator SlugGenerator,
    DateTimeOffset Now);
