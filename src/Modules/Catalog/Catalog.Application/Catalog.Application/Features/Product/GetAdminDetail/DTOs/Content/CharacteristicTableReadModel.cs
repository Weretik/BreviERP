namespace Catalog.Application.Features.Product.GetAdminDetail.DTOs;

public sealed record CharacteristicTableReadModel(string TitleUk, string TitleRu, int SortOrder, IReadOnlyList<CharacteristicRowReadModel> Rows);
