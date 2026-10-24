using Catalog.Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Catalog.Infrastructure.ValueGeneration;

internal sealed class CompositionLineIdValueGenerator : ValueGenerator<CompositionLineId>
{
    private static long _nextTemporaryId;

    public override bool GeneratesTemporaryValues => true;

    public override CompositionLineId Next(EntityEntry entry)
        => CompositionLineId.CreateTemporary(Interlocked.Decrement(ref _nextTemporaryId));
}
