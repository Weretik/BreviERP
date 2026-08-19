namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record InformationBlockRequest(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
