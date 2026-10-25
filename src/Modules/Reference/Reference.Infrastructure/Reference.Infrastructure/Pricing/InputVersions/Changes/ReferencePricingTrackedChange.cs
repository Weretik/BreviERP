using System.Linq.Expressions;
using Reference.Domain.Pricing;

namespace Reference.Infrastructure.Pricing.InputVersions.Changes;

internal sealed record ReferencePricingTrackedChange(
    Expression<Func<ReferencePricingInputVersion, bool>> VersionFilter,
    decimal Value,
    decimal OriginalValue);
