namespace Scheduling.Application.Features.Runs.Commands.FinishScheduledTaskRun;

public sealed class FinishScheduledTaskRunValidator : AbstractValidator<FinishScheduledTaskRunCommand>
{
    public FinishScheduledTaskRunValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
        RuleFor(x => x.RunId).NotEmpty();
        RuleFor(x => x.Message).NotNull();
    }
}
