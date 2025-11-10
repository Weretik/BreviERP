namespace Domain.Reference.ValueObjects;

public sealed record ProfitFactors
{
    public Percent UpTo10Units  { get; init; }
    public Percent TenTo40Units { get; init; }
    public Percent Above40Units { get; init; }

    private ProfitFactors() { }

    private ProfitFactors(decimal upTo10Units, decimal tenTo40Units, decimal above40Units)
    {
        UpTo10Units = Percent.From(upTo10Units);
        TenTo40Units = Percent.From(tenTo40Units);
        Above40Units = Percent.From(above40Units);
    }

    public static ProfitFactors From(decimal upTo10Units, decimal tenTo40Units, decimal above40Units)
        => new(upTo10Units, tenTo40Units, above40Units);

    public static ProfitFactors DefaultProduced => new(55m, 32m, 25m);
    public static ProfitFactors DefaultPpe => new(40m, 33m, 27m);

    public Percent ForQuantity(int quantity)
        => quantity <= 10 ? UpTo10Units
            : quantity <= 40 ? TenTo40Units
            : Above40Units;
}
