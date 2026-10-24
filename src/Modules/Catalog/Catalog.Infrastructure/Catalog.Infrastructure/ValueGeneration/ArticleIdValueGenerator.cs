using Catalog.Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Catalog.Infrastructure.ValueGeneration;

internal sealed class ArticleIdValueGenerator : ValueGenerator<ArticleId>
{
    private static long _nextTemporaryId;

    public override bool GeneratesTemporaryValues => true;

    public override ArticleId Next(EntityEntry entry)
        => ArticleId.CreateTemporary(Interlocked.Decrement(ref _nextTemporaryId));
}
