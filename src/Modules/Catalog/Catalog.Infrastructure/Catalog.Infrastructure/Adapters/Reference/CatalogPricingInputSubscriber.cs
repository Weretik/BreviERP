using Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;
using Reference.Contracts.Pricing;

namespace Catalog.Infrastructure.Adapters.Reference;

/// <summary>A Reference price changed in this request: every Sewing model may need a new price (CAT-012, ADR-0019).</summary>
internal sealed class CatalogPricingInputSubscriber(PricingInputChanges changes) : IPricingInputSubscriber
{
    public void PricingInputsChanged() => changes.MarkAll();
}
