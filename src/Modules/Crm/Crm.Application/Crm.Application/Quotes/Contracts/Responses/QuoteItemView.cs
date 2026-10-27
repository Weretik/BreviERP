namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteItemView(
    Guid PublicId, int SortOrder, string Title, string? PublicDescription, string? PublicComment,
    string? InternalNote, bool Included, IReadOnlyList<QuoteVariantView> Variants);
