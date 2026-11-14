using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;

namespace Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

/// <summary>A product page of the old site: its links in both languages and its texts.</summary>
public sealed record OldSiteProductPage(string Url, string? UrlRu, ProductPageTextInput? Uk, ProductPageTextInput? Ru);
