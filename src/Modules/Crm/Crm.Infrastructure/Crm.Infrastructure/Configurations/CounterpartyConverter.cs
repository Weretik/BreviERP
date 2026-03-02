using Crm.Domain.Enums;
using Crm.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Crm.Infrastructure.Configurations;

public static class CrmConverter
{
    public static readonly ValueConverter<CounterpartyId, int> CounterpartyIdConvert =
    new(
        id => id.Value,
        value => CounterpartyId.From(value)
    );

    public static readonly ValueConverter<CounterpartyType, string> CounterpartyTypeConvert =
        new(
            type => type.Name,
            value => CounterpartyType.FromName(value, false)
        );

}
