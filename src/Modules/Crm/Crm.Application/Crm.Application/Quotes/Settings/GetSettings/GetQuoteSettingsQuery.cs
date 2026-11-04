using Crm.Application.Quotes.Settings.Contracts;

namespace Crm.Application.Quotes.Settings.GetSettings;

/// <summary>«Налаштування КП» with a contact row for every active staff member.</summary>
public sealed record GetQuoteSettingsQuery : IQuery<Result<QuoteSettingsResponse>>;
