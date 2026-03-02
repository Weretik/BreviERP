namespace Accounting.Domain.Errors;

public static class WalletErrors
{
    public static AccountingDomainError IdMustBeProvided() =>
        new("Accounting.Wallet.Id.Required",
            "Wallet id must be provided");

    public static AccountingDomainError NameIsRequired() =>
        new("Accounting.Wallet.Name.Required",
            "Wallet name is required");

    public static AccountingDomainError NameLengthInvalid(int length) =>
        new("Accounting.Wallet.Name.LengthInvalid",
            $"Wallet name length must be between 1 and 100 characters. Actual:{length}");

    public static AccountingDomainError BalanceIsRequired() =>
        new("Accounting.Wallet.Balance.Required",
            "Wallet balance must be provided");

    public static AccountingDomainError AmountMustBePositive(decimal amount) =>
        new("Accounting.Wallet.Amount.Invalid",
            $"Amount must be greater than zero. Actual:{amount}");

    public static AccountingDomainError CurrencyMismatch(string walletCurrency, string operationCurrency) =>
        new("Accounting.Wallet.Currency.Mismatch",
            $"Different currencies are not allowed. Wallet:{walletCurrency}, Operation:{operationCurrency}");
}
