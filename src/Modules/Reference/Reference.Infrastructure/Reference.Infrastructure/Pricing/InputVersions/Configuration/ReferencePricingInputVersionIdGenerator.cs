using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Reference.Domain.Pricing;

namespace Reference.Infrastructure.Pricing.Configuration;

internal sealed class ReferencePricingInputVersionIdGenerator : ValueGenerator<ReferencePricingInputVersionId>
{
    private static long _next;
    public override bool GeneratesTemporaryValues => true;
    public override ReferencePricingInputVersionId Next(EntityEntry entry) =>
        ReferencePricingInputVersionId.Temporary(Interlocked.Decrement(ref _next));
}

