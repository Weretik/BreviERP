namespace Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;

public sealed class GetQuoteFabricFactsValidator : AbstractValidator<GetQuoteFabricFactsQuery>
{
    public const int MaxFabrics = 200;

    public GetQuoteFabricFactsValidator()
    {
        RuleFor(x => x.FabricIds).NotNull()
            .Must(x => x is { Count: > 0 and <= MaxFabrics })
            .WithMessage($"Вкажіть від 1 до {MaxFabrics} тканин.");
        RuleForEach(x => x.FabricIds).GreaterThan(0);
    }
}
