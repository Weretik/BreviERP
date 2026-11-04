using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.GetStaff;

public sealed class GetQuoteStaffQueryHandler(IQuoteStaffReader staff)
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
