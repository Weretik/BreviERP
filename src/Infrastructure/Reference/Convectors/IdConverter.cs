using Domain.Reference.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Reference.Convectors;

public class IdConverter
{
    public static readonly ValueConverter<AdditionalReferenceId, int> AdditionalReferenceIdConvert =
        new(
            id => id.Value,
            value => AdditionalReferenceId.From(value)
        );
}
