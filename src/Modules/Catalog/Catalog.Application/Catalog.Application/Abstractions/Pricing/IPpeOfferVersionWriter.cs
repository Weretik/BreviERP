using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing;
public interface IPpeOfferVersionWriter
{
    Task<Result<PpeOfferVersion>> AppendAsync(int productId, Guid? expectedVersionId, Guid requestId, PpeOfferValues values, string actor, CancellationToken cancellationToken);
}
