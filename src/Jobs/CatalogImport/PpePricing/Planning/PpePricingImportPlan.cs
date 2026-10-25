using Catalog.Application.Contracts.Pricing.Ppe;
namespace CatalogImport.PpePricing.Planning;
public sealed record PpePricingImportPlan(IReadOnlyList<PpeRuleValues> Rules, IReadOnlyDictionary<string,PpeOfferValues> Offers);
