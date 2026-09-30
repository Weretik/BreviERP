using Catalog.Domain.Media.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Catalog.Infrastructure.ValueGeneration;

internal sealed class MediaFileIdValueGenerator : ValueGenerator<MediaFileId>
{
    private static int _nextTemporaryId = int.MinValue;

    public override bool GeneratesTemporaryValues => true;

    public override MediaFileId Next(EntityEntry entry)
        => MediaFileId.CreateTemporary(Interlocked.Increment(ref _nextTemporaryId));
}
