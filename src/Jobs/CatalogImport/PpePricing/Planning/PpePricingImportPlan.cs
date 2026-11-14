using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace CatalogImport.PpePricing.Planning;
public sealed record PpePricingImportPlan(IReadOnlyList<PpeRuleValues> Rules, IReadOnlyDictionary<string,PpeOfferValues> Offers);
