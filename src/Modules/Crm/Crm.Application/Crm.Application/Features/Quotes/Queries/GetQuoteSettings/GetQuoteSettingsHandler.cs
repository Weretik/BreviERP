using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Settings;
using Crm.Application.Features.Quotes.Shared.Settings.Reading;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteSettings;

public sealed class GetQuoteSettingsHandler(IQuoteStore store, IQuoteStaffReader staff)
    : IQueryHandler<GetQuoteSettingsQuery, Result<QuoteSettingsResponse>>
{
    public async ValueTask<Result<QuoteSettingsResponse>> Handle(GetQuoteSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        var members = await staff.ListAsync(cancellationToken);
        return Result.Success(saved.ToResponse(members, await store.MailboxEmailsAsync(cancellationToken)));
    }
}
