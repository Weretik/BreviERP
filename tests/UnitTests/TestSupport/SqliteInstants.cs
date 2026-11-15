using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UnitTests.TestSupport;

/// <summary>
/// SQLite cannot compare or sort DateTimeOffset text; as numbers it can, the way PostgreSQL compares timestamptz.
/// </summary>
internal sealed class SqliteInstantsCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties())
                     .Where(x => x.ClrType == typeof(DateTimeOffset) || x.ClrType == typeof(DateTimeOffset?)))
            property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
    }
}
