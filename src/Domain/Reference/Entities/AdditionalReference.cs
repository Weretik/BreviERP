using Domain.Reference.ValueObjects;

namespace Domain.Reference.Entities;

public class AdditionalReference : BaseEntity<AdditionalReferenceId>, IAggregateRoot
{
    #region Properties
    public int SeamstressCount { get; private set; } = 12;
    public Money SeamstressAverageSalary { get; private set; } = new(15_000, "UAH");
    public Money MonthlyExpenses { get; private set; } = new(180_000, "UAH");
    public LaborOverheadFactors Factors { get; private set; } = LaborOverheadFactors.Default;
    public ProfitFactors ProducedProfit { get; private set; } = ProfitFactors.DefaultProduced;
    public ProfitFactors PpeProfit { get; private set; } = ProfitFactors.DefaultPpe;
    #endregion

    #region Constructors & Factories
    private AdditionalReference() => Id = AdditionalReferenceId.From(1);
    public static AdditionalReference CreateDefault() => new();
    public void Update(
        int seamstressCount,
        Money seamstressAverageSalary,
        Money monthlyExpenses,
        LaborOverheadFactors laborOverheadFactors,
        ProfitFactors producedProfit,
        ProfitFactors ppeProfit)
    {
        SetSeamstressCount(seamstressCount);
        SetSeamstressAverageSalary(seamstressAverageSalary);
        SetMonthlyExpenses(monthlyExpenses);
        SetLaborOverheadFactors(laborOverheadFactors);
        SetProducedProfit(producedProfit);
        SetPpeProfit(ppeProfit);
    }
    #endregion

    #region Setters/Validation
    private void SetSeamstressCount(int count) => SeamstressCount = Guard.Against.OutOfRange(count, nameof(count), 1, 50);
    private void SetSeamstressAverageSalary(Money salary)
    {
        Guard.Against.Null(salary, nameof(salary));
        Guard.Against.OutOfRange(salary.Amount, nameof(salary), 1000, 50_000);
        SeamstressAverageSalary = salary;
    }
    private void SetMonthlyExpenses(Money expenses)
    {
        Guard.Against.Null(expenses, nameof(expenses));
        Guard.Against.OutOfRange(expenses.Amount, nameof(expenses), 100_000, 1_000_000);
        MonthlyExpenses = expenses;
    }
    private void SetLaborOverheadFactors(LaborOverheadFactors factors) => Factors = factors;
    private void SetProducedProfit(ProfitFactors factors) => ProducedProfit = factors;
    private void SetPpeProfit(ProfitFactors factors) => PpeProfit = factors;

    #endregion
}
