using Crm.Application.Features.Leads.Shared;

namespace Crm.Application.Features.Leads.Queries.GetLeadStats;

/// <summary>«Адміністрування › Ліди», tab «Зведення» (R-10). Kyiv dates, both included; default the last 7 days.</summary>
public sealed record GetLeadStatsQuery(DateOnly? From, DateOnly? To) : IQuery<Result<LeadStatsResponse>>;

public sealed class GetLeadStatsValidator : AbstractValidator<GetLeadStatsQuery>
{
    public GetLeadStatsValidator()
    {
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("Початок періоду пізніше за кінець.");
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.To.Value.DayNumber - x.From.Value.DayNumber <= 3660)
            .WithMessage("Період не довший за 10 років.");
    }
}
