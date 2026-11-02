using Reference.Domain.GarmentPartOperations.ValueObjects;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Shared.Specifications;

/// <summary>An operation with this normalized code, optionally other than <paramref name="exceptId"/>.</summary>
public sealed class GarmentPartOperationByCodeSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByCodeSpec(string code, int? exceptId = null)
    {
        Query.Where(x => x.Code == code);
        if (exceptId is { } id)
            Query.Where(x => x.Id != GarmentPartOperationId.From(id));
    }
}
