using System.Security.Cryptography;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Pricing;

namespace Crm.Application.Quotes.Introductions;

/// <summary>Issues actor-bound price bases with stable per-slot request identities.</summary>
public sealed class IntroductionPriceBasisIssuer(IQuotePriceBasisStore bases)
{
    public Task<Result<QuotePricePreview>> IssueAsync(Guid requestId, CrmActor actor,
        IntroductionPreparedSlot slot, CancellationToken cancellationToken)
    {
        var intent = new QuotePriceIntent("sewing", slot.Selection.ProductId,
            slot.Selection.ArticleId, null, null);
        return bases.ExecuteAsync(DeriveRequestId(requestId, slot.Selection.Slot.Uid), null,
            actor, intent, _ => Task.FromResult(Result.Success(new QuotePricePreview(
                Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15), intent,
                slot.CurrentPrice, slot.CurrentPrice.Retail, slot.CurrentPrice.Wholesale,
                slot.CurrentPrice.LargeWholesale, [], "UAH", "catalog_no_extra_vat"))),
            cancellationToken);
    }

    private static Guid DeriveRequestId(Guid requestId, int slotUid)
    {
        Span<byte> input = stackalloc byte[20];
        requestId.TryWriteBytes(input);
        BitConverter.TryWriteBytes(input[16..], slotUid);
        return new Guid(SHA256.HashData(input)[..16]);
    }
}
