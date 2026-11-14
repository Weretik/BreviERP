namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record FabricRequest(int FabricId, bool IsPrimary, int SortOrder);
