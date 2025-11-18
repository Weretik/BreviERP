using Domain.CRM.Enums;
using Domain.CRM.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.CRM.Configurations;

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
