using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Settings.Contracts;

namespace Crm.Application.Quotes.Settings.SaveSettings;

public sealed record SaveQuoteSettingsCommand(QuoteActor Actor, SaveQuoteSettingsInput Input)
    : ICommand<Result<QuoteSettingsResponse>>;
