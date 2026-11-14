namespace Catalog.Application.Features.Product.Shared;

public sealed record InformationBlockAdminDetail(
    string TitleUk,
    string TitleRu,
    string TextUk,
    string TextRu,
    int SortOrder);
