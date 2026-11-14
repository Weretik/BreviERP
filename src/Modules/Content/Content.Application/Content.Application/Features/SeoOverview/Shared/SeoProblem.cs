using System.Text.Json.Serialization;

namespace Content.Application.Features.SeoOverview.Shared;

/// <param name="Language"><c>uk</c> or <c>ru</c>; null for a problem of the whole page.</param>
public sealed record SeoProblem(SeoProblemKind Kind, string? Language);
