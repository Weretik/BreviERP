
using Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition.Results;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition;

public sealed class CalculateSewingCompositionValidator : AbstractValidator<CalculateSewingCompositionQuery>
{
    public const int MaxMaterials = 50;
    public const int MaxOperations = 200;
    public const decimal MaxQuantity = 10_000m;
    public const int MaxCostCodeLength = 80;

    public CalculateSewingCompositionValidator()
    {
        RuleFor(x => x.Materials).NotNull().Must(x => x is { Count: <= MaxMaterials });
        RuleFor(x => x.Operations).NotNull().Must(x => x is { Count: <= MaxOperations });
        RuleForEach(x => x.Materials).NotNull().ChildRules(line =>
        {
            line.RuleFor(x => x.Kind).IsInEnum();
            line.RuleFor(x => x.ReferenceId).GreaterThan(0).When(x => x.Kind != SewingCompositionMaterialKind.OtherCost);
            line.RuleFor(x => x.CostCode).NotEmpty().MaximumLength(MaxCostCodeLength)
                .When(x => x.Kind == SewingCompositionMaterialKind.OtherCost);
            line.RuleFor(x => x.CostCode).Null().When(x => x.Kind != SewingCompositionMaterialKind.OtherCost);
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
