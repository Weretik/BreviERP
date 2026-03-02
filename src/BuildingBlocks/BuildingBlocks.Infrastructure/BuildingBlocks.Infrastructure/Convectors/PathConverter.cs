using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Convectors;

public static class PathConverter
{
    public static readonly ValueConverter<CategoryPath, LTree> Convert =
    new(
        path => (LTree)path.Value,
        value => CategoryPath.From(value)
    );
}
