using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Database;

/// <summary>
/// A generated lower-case <c>SearchText</c> column over the searched columns of a table and its trigram index
/// (database rules 2.1). The database fills it for old and new rows, so no back-fill and no domain code are needed;
/// queries use <c>SearchWords.WhereSearchText</c>.
/// </summary>
public static class SearchTextColumn
{
    public const string Name = "SearchText";

    /// <summary>Enables <c>pg_trgm</c> for the <c>gin_trgm_ops</c> indexes of the context (once per database).</summary>
    public static ModelBuilder AddTrigramSearch(this ModelBuilder builder) => builder.HasPostgresExtension("pg_trgm");

    /// <summary>Adds the column over <paramref name="columns"/> (database column names) and its GIN index.</summary>
    public static EntityTypeBuilder<T> HasSearchText<T>(this EntityTypeBuilder<T> builder, params string[] columns)
        where T : class
    {
        builder.Property<string>(Name).HasComputedColumnSql(Sql(columns), stored: true);
        builder.HasIndex(Name).HasMethod("gin").HasOperators("gin_trgm_ops");
        return builder;
    }

    /// <summary>
    /// <c>lower</c> of the columns joined by a space, with <c>’</c> and <c>ʼ</c> as <c>'</c>; only immutable functions, as
    /// a PostgreSQL generated column requires (and valid in SQLite for unit tests).
    /// </summary>
    public static string Sql(params string[] columns)
    {
        if (columns.Length == 0) throw new ArgumentException("At least one column.", nameof(columns));
        var joined = string.Join(" || ' ' || ", columns.Select(x => $"coalesce(\"{x}\", '')"));
        return $"replace(replace(lower({joined}), '’', ''''), 'ʼ', '''')";
    }
}
