namespace Catalog.Application.Features.Product.Commands.CreateProduct.Content;

public sealed record InformationBlockRequest(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
