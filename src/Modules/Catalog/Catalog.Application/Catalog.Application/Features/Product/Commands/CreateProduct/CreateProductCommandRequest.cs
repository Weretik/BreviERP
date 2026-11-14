using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record CreateProductCommandRequest(
    int Id,
    string Name,
    string RuName,
    ProductType Type,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyCollection<int> CategoryIds,
    IReadOnlyCollection<ProductPhotoRequest> Photos,
    IReadOnlyCollection<InformationBlockRequest> InformationBlocks,
    IReadOnlyCollection<CharacteristicTableRequest> CharacteristicTables,
    SewingProductRequest? Sewing,
    PpeProductRequest? Ppe);
