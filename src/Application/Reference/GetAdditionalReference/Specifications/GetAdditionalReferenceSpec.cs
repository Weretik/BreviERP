using Domain.Reference.Entities;

namespace Application.Reference.GetAdditionalReference.Specifications;

public class GetAdditionalReferenceSpec : Specification<AdditionalReference, AdditionalReferenceDTO>
{
    public GetAdditionalReferenceSpec()
    {
        Query.AsNoTracking()
            .Where(x => x.Id == 1)
            .Select(x => new AdditionalReferenceDTO(
                x.SeamstressCount,
                x.SeamstressAverageSalary.Amount,
                x.MonthlyExpenses.Amount,
                new LaborOverheadFactorsDTO(
                    x.Factors.Cutting.Value,
                    x.Factors.Master.Value,
                    x.Factors.WorkshopManager.Value,
                    x.Factors.SeamstressBonus.Value),
                new ProfitFactorsDTO(
                    x.ProducedProfit.Above40Units.Value,
                    x.ProducedProfit.TenTo40Units.Value,
                    x.ProducedProfit.UpTo10Units.Value),
                new ProfitFactorsDTO(
                    x.PpeProfit.Above40Units.Value,
                    x.PpeProfit.TenTo40Units.Value,
                    x.PpeProfit.UpTo10Units.Value)
                ));
    }
}
