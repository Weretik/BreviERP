using Ardalis.Result;
using Catalog.Application.Features.Product.Commands.CreateProduct;
using Catalog.Application.Features.Product.Commands.CreateProduct.Ppe;

namespace Catalog.Api.Endpoints.Product.Write.Ppe;

internal sealed record PpeProductWriteMappingResult(
    PpeProductRequest Value,
    IReadOnlyCollection<ValidationError> ValidationErrors);
