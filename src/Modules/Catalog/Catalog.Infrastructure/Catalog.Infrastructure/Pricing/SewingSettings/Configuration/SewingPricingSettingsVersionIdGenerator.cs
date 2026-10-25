using Catalog.Domain.Pricing.Sewing;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Catalog.Infrastructure.Pricing.Configuration;

internal sealed class SewingPricingSettingsVersionIdGenerator : ValueGenerator<SewingPricingSettingsVersionId>
{
    private static long _next;
    public override bool GeneratesTemporaryValues => true;
    public override SewingPricingSettingsVersionId Next(EntityEntry entry) =>
        SewingPricingSettingsVersionId.Temporary(Interlocked.Decrement(ref _next));
}

