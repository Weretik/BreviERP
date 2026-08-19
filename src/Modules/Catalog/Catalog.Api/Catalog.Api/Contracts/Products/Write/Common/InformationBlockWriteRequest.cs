namespace Catalog.Api.Contracts.Products;

public sealed record InformationBlockWriteRequest(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
