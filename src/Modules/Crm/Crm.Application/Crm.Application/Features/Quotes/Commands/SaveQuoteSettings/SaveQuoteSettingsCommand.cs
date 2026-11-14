using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Settings;

namespace Crm.Application.Features.Quotes.Commands.SaveQuoteSettings;

public sealed record SaveQuoteSettingsCommand(QuoteActor Actor, SaveQuoteSettingsInput Input)
    : ICommand<Result<QuoteSettingsResponse>>;
