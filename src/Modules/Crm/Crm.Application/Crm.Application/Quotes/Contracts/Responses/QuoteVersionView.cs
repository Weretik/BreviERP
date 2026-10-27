namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteVersionView(
    Guid PublicId, int VersionNumber, string Action, string RecipientName,
    Guid? CustomerPublicId, Guid? ContactPublicId, string Currency, string TaxBasis, int ActorId,
    DateTimeOffset CreatedAt, IReadOnlyList<QuoteItemView> Items);
