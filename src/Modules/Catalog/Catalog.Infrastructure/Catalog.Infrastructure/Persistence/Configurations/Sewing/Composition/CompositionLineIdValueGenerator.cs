using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Catalog.Domain.Products.Sewing.ValueObjects;


namespace Catalog.Infrastructure.Persistence.Configurations.Sewing.Composition;

internal sealed class CompositionLineIdValueGenerator : ValueGenerator<CompositionLineId>
{
    private static long _nextTemporaryId;

    public override bool GeneratesTemporaryValues => true;

    public override CompositionLineId Next(EntityEntry entry)
        => CompositionLineId.CreateTemporary(Interlocked.Decrement(ref _nextTemporaryId));
}
