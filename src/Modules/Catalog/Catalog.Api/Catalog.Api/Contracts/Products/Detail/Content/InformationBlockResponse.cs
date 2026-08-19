namespace Catalog.Api.Contracts.Products;

public sealed record InformationBlockResponse(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
