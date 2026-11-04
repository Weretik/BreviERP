namespace Crm.Application.Tasks.GetTaskOverview;

public sealed class GetTaskOverviewQueryValidator : AbstractValidator<GetTaskOverviewQuery>
{
    public GetTaskOverviewQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.AssigneeUserId).GreaterThan(0).When(x => x.AssigneeUserId is not null);
    }
}
