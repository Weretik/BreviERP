using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record PercentRequest(PricePercentSource Source, int? AdditionalReferenceId, decimal? CustomPercent);
