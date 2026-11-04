namespace Catalog.Application.Features.Pricing.Sewing.Composition;

public sealed class CalculateSewingCompositionQueryValidator : AbstractValidator<CalculateSewingCompositionQuery>
{
    public const int MaxMaterials = 50;
    public const int MaxOperations = 200;
    public const decimal MaxQuantity = 10_000m;

    public CalculateSewingCompositionQueryValidator()
    {
        RuleFor(x => x.Materials).NotNull().Must(x => x is { Count: <= MaxMaterials });
        RuleFor(x => x.Operations).NotNull().Must(x => x is { Count: <= MaxOperations });
        RuleForEach(x => x.Materials).NotNull().ChildRules(line =>
        {
            line.RuleFor(x => x.Kind).IsInEnum();
            line.RuleFor(x => x.ReferenceId).GreaterThan(0);
            line.RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(MaxQuantity);
        });
        RuleForEach(x => x.Operations).NotNull().ChildRules(line =>
        {
            line.RuleFor(x => x.OperationId).GreaterThan(0);
            line.RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(MaxQuantity);
            line.RuleFor(x => x.ManualMinutes).GreaterThan(0).LessThanOrEqualTo(MaxQuantity)
                .When(x => x.ManualMinutes is not null);
        });
    }
}
