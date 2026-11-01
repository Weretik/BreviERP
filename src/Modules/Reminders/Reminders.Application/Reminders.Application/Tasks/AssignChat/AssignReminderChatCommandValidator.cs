using Reminders.Application.Contracts;

namespace Reminders.Application.Tasks.AssignChat;

public sealed class AssignReminderChatCommandValidator : AbstractValidator<AssignReminderChatCommand>
{
    public AssignReminderChatCommandValidator()
    {
        RuleFor(x => x.ChatId).GreaterThan(0);
        RuleFor(x => x.Organizations).NotEmpty();
        RuleForEach(x => x.Organizations).Must(ReminderCodes.Organizations.ContainsKey)
            .WithMessage("Організація: none, ppBrevi або fopSohokon.");
    }
}
