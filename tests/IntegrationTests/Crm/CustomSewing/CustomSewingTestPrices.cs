using Crm.Application.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;

namespace IntegrationTests.Crm.CustomSewing;

internal sealed class CustomSewingTestPrices : ICustomSewingPricing
{
    public long SourceVersion { get; set; } = 1;
    public bool Blocked { get; set; }
    public bool Expired { get; set; }
    public int Calls { get; private set; }
    public Task<CustomPriceBasis> CalculateAsync(CustomConfiguration config, DateTimeOffset at, CancellationToken ct)
    {
        Calls++;
        var amount = config.Fabrics.Sum(x => x.Quantity) * 100m;
        return Task.FromResult(new CustomPriceBasis(Guid.NewGuid(), at, at.AddMinutes(Expired ? -1 : 15),
            "custom-sewing-sheet-v1", 10, [new("Fabric:1", SourceVersion, 100, "Meters", "synthetic"),
            new("Operation:2", 2, 10, "Minutes", "synthetic")],
            new(amount, 0, 10, 1, 10, 0, 0, 0, 0, 0, config.Delivery, config.Packaging, 0, 0),
            Blocked ? null : amount / 2, Blocked ? null : amount, Blocked ? null : amount * .9m,
            Blocked ? null : amount * .8m, Blocked ? ["UnapprovedReference"] : []));
    }
}
