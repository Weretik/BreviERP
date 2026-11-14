namespace Crm.Application.Features.Tasks.Queries.GetTaskOverview;

public sealed class GetTaskOverviewValidator : AbstractValidator<GetTaskOverviewQuery>
{
    public GetTaskOverviewValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.AssigneeUserId).GreaterThan(0).When(x => x.AssigneeUserId is not null);
    }
}
