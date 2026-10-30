using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Application.CustomSewing.Persistence;
using Crm.Application.CustomSewing.GetDraft.Specifications;

namespace Crm.Application.CustomSewing.GetDraft;

public sealed class GetCustomDraftQueryHandler(ICustomReadRepository repository)
    : IQueryHandler<GetCustomDraftQuery, Result<CustomDetail>>
{
    public async ValueTask<Result<CustomDetail>> Handle(GetCustomDraftQuery query, CancellationToken cancellationToken)
    {
        var detail = await repository.FirstOrDefaultAsync(new CustomDraftDetailSpec(query.DraftId, query.Actor), cancellationToken);
        return detail is null ? Result.NotFound() : Result.Success(detail);
    }
}
