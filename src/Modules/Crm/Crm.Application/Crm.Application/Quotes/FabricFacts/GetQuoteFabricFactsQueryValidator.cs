namespace Crm.Application.Quotes.FabricFacts;

public sealed class GetQuoteFabricFactsQueryValidator : AbstractValidator<GetQuoteFabricFactsQuery>
{
    public const int MaxFabrics = 200;

    public GetQuoteFabricFactsQueryValidator()
    {
        RuleFor(x => x.FabricIds).NotNull()
            .Must(x => x is { Count: > 0 and <= MaxFabrics })
            .WithMessage($"Вкажіть від 1 до {MaxFabrics} тканин.");
        RuleForEach(x => x.FabricIds).GreaterThan(0);
    }
}
