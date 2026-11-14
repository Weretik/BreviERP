namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail.Content;

public sealed record CharacteristicTableReadModel(string TitleUk, string TitleRu, int SortOrder, IReadOnlyList<CharacteristicRowReadModel> Rows);
