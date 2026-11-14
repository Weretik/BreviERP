using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Catalog.Domain.Products.Sewing.ValueObjects;

namespace Catalog.Infrastructure.Persistence.Products;

internal sealed class ArticleIdValueGenerator : ValueGenerator<ArticleId>
{
    private static long _nextTemporaryId;

    public override bool GeneratesTemporaryValues => true;

    public override ArticleId Next(EntityEntry entry)
        => ArticleId.CreateTemporary(Interlocked.Decrement(ref _nextTemporaryId));
}
