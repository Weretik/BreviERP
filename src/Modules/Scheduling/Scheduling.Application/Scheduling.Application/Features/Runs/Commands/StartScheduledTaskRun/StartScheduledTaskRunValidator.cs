namespace Scheduling.Application.Features.Runs.Commands.StartScheduledTaskRun;

public sealed class StartScheduledTaskRunValidator : AbstractValidator<StartScheduledTaskRunCommand>
{
    public StartScheduledTaskRunValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
        RuleFor(x => x.RunId).NotEmpty();
    }
}
