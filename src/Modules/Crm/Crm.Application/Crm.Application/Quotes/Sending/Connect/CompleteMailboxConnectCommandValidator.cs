namespace Crm.Application.Quotes.Sending.Connect;

/// <summary>Values come from Google through the browser; only their size is limited, the handler checks the state.</summary>
public sealed class CompleteMailboxConnectCommandValidator : AbstractValidator<CompleteMailboxConnectCommand>
{
    public CompleteMailboxConnectCommandValidator()
    {
        RuleFor(x => x.Code).MaximumLength(2000);
        RuleFor(x => x.State).MaximumLength(4000);
        RuleFor(x => x.Error).MaximumLength(200);
    }
}
