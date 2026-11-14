namespace Catalog.Api.Endpoints.Product.Write.Content;

public sealed record InformationBlockWriteRequest(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
