using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteStaff;

public sealed class GetQuoteStaffHandler(IQuoteStaffReader staff)
    : IQueryHandler<GetQuoteStaffQuery, Result<IReadOnlyList<QuoteStaffResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<QuoteStaffResponse>>> Handle(GetQuoteStaffQuery query,
        CancellationToken cancellationToken)
    {
        var members = await staff.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<QuoteStaffResponse>>(
            members.Select(x => new QuoteStaffResponse(x.UserId, x.Name, x.Email)).ToArray());
    }
}
