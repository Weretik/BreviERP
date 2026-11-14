using Catalog.Domain.Products;
using Catalog.Domain.Products.Ppe.Pricing;

namespace Catalog.Application.Features.Product.Commands.CreateProduct.Ppe;

public sealed record PercentRequest(PricePercentSource Source, int? AdditionalReferenceId, decimal? CustomPercent);
