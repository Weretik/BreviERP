namespace Reference.Infrastructure.Congigurations;

public static class ReferenceConverters
{
    public static readonly ValueConverter<AdditionalReferenceId, int> AdditionalReferenceIdConvert =
        new(
            id => id.Value,
            v => AdditionalReferenceId.From(v)
        );
    public static readonly ValueConverter<FabricId, int> FabricIdConvert =
        new(
            id => id.Value,
            v => FabricId.From(v)
        );

    public static readonly ValueConverter<Percent, decimal> PercentConvert =
        new(
            p => p.Value,
            v => Percent.From(v)
        );


    public static readonly ValueConverter<Money, decimal> MoneyConvert =
        new(
            m => m.Amount,
            v => new Money(v, "UAH")
        );
}
