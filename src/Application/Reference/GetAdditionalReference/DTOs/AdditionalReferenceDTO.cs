namespace Application.Reference.GetAdditionalReference;

public sealed record AdditionalReferenceDTO(
    int SeamstressCount,
    decimal SeamstressAverageSalary,
    decimal MonthlyExpenses,
    LaborOverheadFactorsDTO Factors,
    ProfitFactorsDTO ProducedProfit,
    ProfitFactorsDTO PpeProfit);
