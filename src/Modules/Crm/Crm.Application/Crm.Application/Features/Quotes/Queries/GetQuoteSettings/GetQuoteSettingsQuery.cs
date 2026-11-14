using Crm.Application.Features.Quotes.Shared.Settings;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteSettings;

/// <summary>«Налаштування КП» with a contact row for every active staff member.</summary>
public sealed record GetQuoteSettingsQuery : IQuery<Result<QuoteSettingsResponse>>;
