using Catalog.Application.Abstractions.Pricing;
namespace CatalogImport.PpePricing.Planning;
public sealed record PpePricingImportPlan(IReadOnlyList<PpeRuleValues> Rules, IReadOnlyDictionary<string,PpeOfferValues> Offers);
