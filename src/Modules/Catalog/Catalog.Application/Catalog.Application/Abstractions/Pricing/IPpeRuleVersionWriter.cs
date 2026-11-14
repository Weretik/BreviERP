using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing;
public interface IPpeRuleVersionWriter
{
    Task<Result<PpeRuleVersion>> AppendAsync(Guid? expectedVersionId, Guid requestId, PpeRuleValues values, string actor, CancellationToken cancellationToken);
}
