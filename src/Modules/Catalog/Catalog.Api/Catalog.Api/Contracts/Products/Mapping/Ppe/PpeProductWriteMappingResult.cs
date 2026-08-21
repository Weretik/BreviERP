using Ardalis.Result;
using Catalog.Application.Features.Product.Create.DTOs;

namespace Catalog.Api.Contracts.Products;

internal sealed record PpeProductWriteMappingResult(
    PpeProductRequest Value,
    IReadOnlyCollection<ValidationError> ValidationErrors);
