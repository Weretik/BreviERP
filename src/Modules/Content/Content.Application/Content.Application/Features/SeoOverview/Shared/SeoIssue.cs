using System.Text.Json.Serialization;

namespace Content.Application.Features.SeoOverview.Shared;

/// <param name="Id">The product, category or article id, or the page key.</param>
/// <param name="Path">The address on the site without the language.</param>
public sealed record SeoIssue(SeoArea Area, string Id, string Name, string Path, IReadOnlyList<SeoProblem> Problems);
