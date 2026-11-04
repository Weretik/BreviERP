using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Settings.Contracts;
using Crm.Application.Quotes.Settings.Reading;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.Settings.GetSettings;

public sealed class GetQuoteSettingsQueryHandler(IQuoteStore store, IQuoteStaffReader staff)
    : IQueryHandler<GetQuoteSettingsQuery, Result<QuoteSettingsResponse>>
{
    public async ValueTask<Result<QuoteSettingsResponse>> Handle(GetQuoteSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        var members = await staff.ListAsync(cancellationToken);
        return Result.Success(saved.ToResponse(members));
    }
}
