using Ardalis.Result;
using Catalog.Application.Features.Product.Commands.CreateProduct;

namespace Catalog.Api.Product.Write.Ppe;

internal sealed record PpeProductWriteMappingResult(
    PpeProductRequest Value,
    IReadOnlyCollection<ValidationError> ValidationErrors);
