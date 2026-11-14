using BuildingBlocks.Domain.Errors;

namespace Accounting.Domain.Errors;

/// <summary>A broken finance rule; the message is shown to the user as is.</summary>
public static class AccountingDomainError
{
    public static DomainException Fail(string code, string message) => new(Error.Validation(code, message));
}
