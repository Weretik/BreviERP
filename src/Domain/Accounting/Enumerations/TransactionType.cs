namespace Domain.Accounting.Enumerations;

public sealed class TransactionType(string name, int value, string description, char symbol ) : SmartEnum<TransactionType>(name, value)
{
    public string Description { get; } = description;
    public char Symbol { get; } = symbol;

    public static readonly TransactionType Income  = new("Дохід", 1, "Операція, для збільшення балансу", '+' );
    public static readonly TransactionType Expense = new("Витрата", 2, "Операція, для зменшення балансу", '-');
}
