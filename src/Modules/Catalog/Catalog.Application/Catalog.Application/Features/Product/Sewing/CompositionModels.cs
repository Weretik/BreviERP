using System.Text.Json.Serialization;

namespace Catalog.Application.Features.Product.Sewing;

public sealed record CreateSewingArticleRequest(string SaleSku, bool Active, int SortOrder, [property: JsonRequired] long ExpectedVersion);
public sealed record SewingMaterialInput(string Kind, int? ReferenceId, string? CostCode, decimal Quantity,
    string Unit, bool Required, int SortOrder, bool Reviewed);
public sealed record SewingOperationInput(int OperationId, decimal Quantity, decimal? ManualMinutes,
    int SortOrder, bool Reviewed);
public sealed record ReplaceSewingCompositionRequest(string? ModelCode, IReadOnlyList<SewingMaterialInput> Materials,
    IReadOnlyList<SewingOperationInput> Operations, [property: JsonRequired] long ExpectedVersion);
public sealed record ReplaceSewingArticleCompositionRequest(IReadOnlyList<SewingMaterialInput> Materials, [property: JsonRequired] long ExpectedVersion);
public sealed record SewingArticleView(long Id, int ProductId, string SaleSku, bool Active, int SortOrder);
public sealed record SewingArticleList(int ProductId, string? ModelCode, long Version, IReadOnlyList<SewingArticleView> Articles);
public sealed record SewingMaterialView(long Id, long? ArticleId, string Scope, string Kind, int? ReferenceId,
    string? CostCode, decimal Quantity, string Unit, bool Required, int SortOrder, bool Reviewed,
    string? ReviewedBy, DateTimeOffset? ReviewedAt);
public sealed record SewingOperationView(long Id, int OperationId, decimal Quantity, decimal? ManualMinutes,
    int SortOrder, bool Reviewed, string? ReviewedBy, DateTimeOffset? ReviewedAt);
public sealed record SewingComposition(int ProductId, long? ArticleId, string? ModelCode, long Version,
    IReadOnlyList<SewingMaterialView> Materials, IReadOnlyList<SewingOperationView> Operations,
    IReadOnlyList<string> Blockers);
