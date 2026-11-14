using Ardalis.Result;
using Catalog.Application.Features.Product.Commands.CreateProduct;

namespace Catalog.Api.Endpoints.Product.Write;

internal sealed record PpeProductWriteMappingResult(
    PpeProductRequest Value,
    IReadOnlyCollection<ValidationError> ValidationErrors);
