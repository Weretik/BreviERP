namespace Domain.Accounting.Enumerations;

public abstract class TransactionType(string name, int value) : SmartEnum<TransactionType>(name, value)
{
    public abstract string TypeName { get; }
    public abstract string Description { get; }
    public abstract char Symbol { get; }

    public static readonly TransactionType Income  = new IncomeType();
    public static readonly TransactionType Expense = new ExpenseType();

    private sealed class IncomeType() : TransactionType(nameof(Income), 1)
    {
        public override string TypeName => "Дохід";
        public override string Description => "Операція, для збільшення балансу";
        public override char Symbol => '+';
    }

    private sealed class ExpenseType() : TransactionType(nameof(Expense), 2)
    {
        public override string TypeName => "Витрата";
        public override string Description => "Операція, для зменшення балансу";
        public override char Symbol => '-';
    }
}
