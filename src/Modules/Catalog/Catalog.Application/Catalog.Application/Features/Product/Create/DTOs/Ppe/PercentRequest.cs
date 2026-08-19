using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record PercentRequest(PricePercentSource Source, int? AdditionalReferenceId, decimal? CustomPercent);
