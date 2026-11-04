namespace Crm.Application.Quotes.CustomSewing;

/// <summary>«Змінити склад»: the composition of a Catalog sewing model to start from (R-21). Nothing is saved.</summary>
public sealed record GetQuoteModelCompositionQuery(int ProductId) : IQuery<Result<QuoteModelCompositionResponse>>;

public sealed record QuoteModelCompositionResponse(
    int ProductId, IReadOnlyList<QuoteModelFabricResponse> Fabrics, IReadOnlyList<QuoteModelLineResponse> Lines);

public sealed record QuoteModelFabricResponse(long ArticleId, int FabricId, string? Name, decimal Meters, bool IsActive);

public sealed record QuoteModelLineResponse(
    string Kind, int ReferenceId, string? CostCode, string? Name, string Unit, decimal Quantity, decimal? ManualMinutes);
