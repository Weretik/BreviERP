namespace CatalogImport.Persistence;

public sealed class SourceMappingWriter(CatalogImportDbContext db, string snapshotHash)
{
    public void Add(string dataset, string sourceKey, string targetType, long targetId)
    {
        db.Mappings.Add(new SourceIdMapping
        {
            Dataset = dataset,
            SourceKey = sourceKey,
            TargetType = targetType,
            TargetId = targetId,
            SnapshotHash = snapshotHash
        });
    }
}

