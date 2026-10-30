namespace Crm.Domain.CustomSewing.Composition;

public sealed record CustomConfiguration(
    string Name, string ProductType, string Season, string Description, string ClientComment, string InternalNote,
    IReadOnlyList<CustomMaterial> Fabrics, IReadOnlyList<CustomMaterial> Accessories,
    IReadOnlyList<CustomOperation> Operations, IReadOnlyList<CustomApplication> Applications,
    decimal Delivery, decimal Packaging)
{
    public CustomConfiguration Freeze()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 300 || string.IsNullOrWhiteSpace(ProductType)
            || ProductType.Length > 100 || Season is not ("summer" or "winter")
            || Description is null || Description.Length > 5000 || ClientComment is null || ClientComment.Length > 5000
            || InternalNote is null || InternalNote.Length > 5000 || Fabrics is null || Fabrics.Count > 40
            || Accessories is null || Accessories.Count > 100 || Operations is null || Operations.Count > 100
            || Applications is null || Applications.Count > 20 || !Money(Delivery) || !Money(Packaging))
            throw new CustomSewingValidationException("invalid_custom_configuration");
        if (Fabrics.Concat(Accessories).Any(x => x is null || x.ReferenceId <= 0 || !Norm(x.Quantity)
                || string.IsNullOrWhiteSpace(x.Unit) || x.Unit.Length > 50)
            || Operations.Any(x => x is null || x.ReferenceId <= 0 || !Norm(x.Quantity)
                || (x.ManualMinutes is { } minutes && !Norm(minutes)))
            || Applications.Any(x => x is null || x.Kind is not ("embroidery" or "print")
                || x.Location is null || x.Location.Length > 200 || x.ArtworkFileId == Guid.Empty
                || !Money(x.Cost) || !Money(x.SaleAmount)))
            throw new CustomSewingValidationException("invalid_custom_composition");
        return this with { Name = Name.Trim(), ProductType = ProductType.Trim(),
            Fabrics = Array.AsReadOnly(Fabrics.ToArray()), Accessories = Array.AsReadOnly(Accessories.ToArray()),
            Operations = Array.AsReadOnly(Operations.ToArray()), Applications = Array.AsReadOnly(Applications.ToArray()) };
    }

    private static bool Money(decimal value) => value >= 0 && value <= 10000000m && decimal.Round(value, 2) == value;
    private static bool Norm(decimal value) => value > 0 && value <= 100000 && decimal.Round(value, 6) == value;
}
