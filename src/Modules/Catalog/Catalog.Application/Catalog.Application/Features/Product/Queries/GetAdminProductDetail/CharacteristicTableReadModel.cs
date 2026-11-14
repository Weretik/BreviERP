namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed record CharacteristicTableReadModel(string TitleUk, string TitleRu, int SortOrder, IReadOnlyList<CharacteristicRowReadModel> Rows);
