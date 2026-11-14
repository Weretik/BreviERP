using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Commands.AssignReminderChat;

public sealed class AssignReminderChatValidator : AbstractValidator<AssignReminderChatCommand>
{
    public AssignReminderChatValidator()
    {
        RuleFor(x => x.ChatId).GreaterThan(0);
        RuleFor(x => x.Organizations).NotEmpty();
        RuleForEach(x => x.Organizations).Must(ReminderCodes.Organizations.ContainsKey)
            .WithMessage("Організація: none, ppBrevi або fopSohokon.");
    }
}
