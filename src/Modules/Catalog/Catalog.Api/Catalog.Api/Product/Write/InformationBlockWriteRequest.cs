namespace Catalog.Api.Product.Write;

public sealed record InformationBlockWriteRequest(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
