namespace Accounting.Domain.Errors;

/// <summary>A broken finance rule; the message is shown to the user as is.</summary>
public sealed record AccountingDomainError(string Code, string Message) : IDomainError
{
    public static DomainException Fail(string code, string message) => new(new AccountingDomainError(code, message));
}
