using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Contracts.Pricing.Ppe;
public interface IPpeRuleVersionWriter
{
    Task<Result<PpeRuleVersion>> AppendAsync(Guid? expectedVersionId, Guid requestId, PpeRuleValues values, string actor, CancellationToken cancellationToken);
}
