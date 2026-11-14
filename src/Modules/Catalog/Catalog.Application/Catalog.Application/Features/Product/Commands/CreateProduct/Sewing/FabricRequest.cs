namespace Catalog.Application.Features.Product.Commands.CreateProduct.Sewing;

public sealed record FabricRequest(int FabricId, bool IsPrimary, int SortOrder);
