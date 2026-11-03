namespace Catalog.Application.Features.Product.SheetImport.DTOs;

/// <summary>A site article of a sewing model (tab «Прайс на сайт»).</summary>
public sealed record SheetSewingArticleRow(string SaleSku, bool IsActive);
