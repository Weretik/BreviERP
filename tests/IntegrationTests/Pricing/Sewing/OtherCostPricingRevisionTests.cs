using Ardalis.Result;
using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.Pricing;
using SewingPricing.Jobs.Bootstrap;
using Reference.Application.Features.Pricing.Commands.ReviseOtherCost;
using Reference.Infrastructure.Persistence.Pricing;

namespace IntegrationTests.Pricing.Sewing;

[NonParallelizable]
public sealed class OtherCostPricingRevisionTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Other_cost_revision_is_append_only_idempotent_and_uses_the_expected_version()
    {
        await using var reference = Reference();
        var baseline = ReferencePricingInputVersion.Create(null, null, null, "SERVICE-TEST", 1,
            PricingInputUnit.UahPerService, true, true, 1, DateTimeOffset.UtcNow.AddDays(-1),
            "SERVICE-TEST", new string('E', 64), null);
        reference.Add(baseline);
        await reference.SaveChangesAsync();
        var handler = new ReviseOtherCostHandler(new OtherCostPricingVersionWriter(reference));
        var request = new ReviseOtherCostCommand("SERVICE-TEST", baseline.Id.Value, 2, Guid.NewGuid());
        var result = await handler.Handle(request, CancellationToken.None);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That((await handler.Handle(request, CancellationToken.None)).Value, Is.EqualTo(result.Value));
        Assert.That((await handler.Handle(request with { Value = 3 }, CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await handler.Handle(request with { RequestId = Guid.NewGuid() }, CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(new ReviseOtherCostValidator().Validate(request with { Value = -1 }).IsValid, Is.False);
        Assert.That(await reference.Set<ReferencePricingInputVersion>().CountAsync(x => x.OtherCostCode == "SERVICE-TEST"), Is.EqualTo(2));
        Assert.That((await reference.Set<ReferencePricingInputVersion>().AsNoTracking().SingleAsync(x => x.Id == baseline.Id)).Value,
            Is.EqualTo(1));
    }
}
