using Crm.Domain.Leads;

namespace Crm.Application.Leads.GetLeads;

public sealed class GetLeadsQueryValidator : AbstractValidator<GetLeadsQuery>
{
    public GetLeadsQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.Category).MaximumLength(LeadLimits.CategoryMaxLength);
        RuleFor(x => x.Scope).Must(x => x is null or "All" or "Mine" or "Free")
            .WithMessage("Невідомий фільтр: очікується All, Mine або Free.");
        RuleFor(x => x.Stage).Must(x => x is null or "Open" || Enum.TryParse<LeadStage>(x, out _))
            .WithMessage("Невідомий етап ліда.");
    }
}
