namespace Crm.Application.Tasks.GetTaskBoard;

public sealed class GetTaskBoardQueryValidator : AbstractValidator<GetTaskBoardQuery>
{
    public GetTaskBoardQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.AssigneeUserId).GreaterThan(0).When(x => x.AssigneeUserId is not null);
    }
}
