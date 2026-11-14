using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing;
public sealed record PpeOfferValues(string? RuleCode, decimal? SupplierCost, decimal? ExternalRetail, decimal? ExternalWholesale, string Source, string SourceDate);
