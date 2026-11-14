using BuildingBlocks.Domain.Errors;

namespace Accounting.Domain;

/// <summary>A broken finance rule; the message is shown to the user as is. Rules return it in a DomainResult.</summary>
public static class AccountingErrors
{
    public static Error Rule(string code, string message) => Error.Validation(code, message);
}
