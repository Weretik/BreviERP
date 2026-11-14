namespace Crm.Application.Features.Tasks.Queries.GetTask;

public sealed class GetTaskValidator : AbstractValidator<GetTaskQuery>
{
    public GetTaskValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
