using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;

/// <summary>A block of an old-site article; a picture is named by its address on the old site.</summary>
public sealed record OldSiteArticleBlock(
    ArticleBlockKind Kind,
    string? Text,
    string? Lead,
    IReadOnlyList<string>? Items,
    string? ImageUrl = null,
    string? Alt = null);
