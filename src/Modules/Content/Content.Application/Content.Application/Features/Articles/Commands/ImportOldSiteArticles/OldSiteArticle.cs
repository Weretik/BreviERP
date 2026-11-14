using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

/// <summary>An article of the old site: its address, date, the address of its cover and texts in both languages.</summary>
public sealed record OldSiteArticle(
    string Slug,
    DateOnly PublishedOn,
    string? CoverUrl,
    OldSiteArticleText? Uk,
    OldSiteArticleText? Ru);
