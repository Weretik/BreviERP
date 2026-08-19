namespace Catalog.Application.Contracts.Admin.Product;

public sealed record InformationBlockAdminDetail(
    string TitleUk,
    string TitleRu,
    string TextUk,
    string TextRu,
    int SortOrder);
