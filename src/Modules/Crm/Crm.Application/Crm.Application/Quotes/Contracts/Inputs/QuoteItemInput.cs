namespace Crm.Application.Quotes.Contracts.Inputs;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuoteItemInput(
    Guid? PublicId,
    int SortOrder,
    string Title,
    string? PublicDescription,
    string? PublicComment,
    string? InternalNote,
    bool Included,
    IReadOnlyList<QuoteVariantInput> Variants);
