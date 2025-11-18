using Application.Reference.Shared;
using Domain.Reference.Entities;

namespace Application.Reference.GetAdditionalReference;

public class GetAdditionalReferenceQueryHandler(IReferenceReadRepository<AdditionalReference> repository)
    : IQueryHandler<GetAdditionalReferenceQuery, Result<List<AdditionalReferenceRowDTO>>>
{
    public async ValueTask<Result<List<AdditionalReferenceRowDTO>>> Handle(
        GetAdditionalReferenceQuery query, CancellationToken cancellationToken)
    {
        var referenceSpec = new GetAdditionalReferenceSpec();
        var addRef = await repository.FirstOrDefaultAsync(referenceSpec, cancellationToken);

        if (addRef is null) return Result.NotFound();

        var result = new List<AdditionalReferenceRowDTO>
        {
            new ("Кількість швачок", addRef.SeamstressCount, "шт"),
            new ("Середня зарплата швачки", addRef.SeamstressAverageSalary.Amount, "грн"),
            new ("Витрати за місяць", addRef.MonthlyExpenses.Amount, "грн"),

            new ("Відсоток закрою", addRef.Factors.Cutting.Value, "%"),
            new ("Відсоток майстра", addRef.Factors.Master.Value, "%"),
            new ("Відсоток начальниці цеху", addRef.Factors.WorkshopManager.Value, "%"),
            new ("Відсоток премії швачки", addRef.Factors.SeamstressBonus.Value, "%"),

            new ("Прибуток до 10 шт", addRef.ProducedProfit.UpTo10Units.Value, "%"),
            new ("Прибуток 10-40 шт", addRef.ProducedProfit.TenTo40Units.Value, "%"),
            new ("Прибуток понад 40 шт ", addRef.ProducedProfit.Above40Units.Value, "%"),

            new ("Прибуток (ЗІЗ) до 10 шт", addRef.PpeProfit.UpTo10Units.Value, "%"),
            new ("Прибуток (ЗІЗ) 10-40 шт", addRef.PpeProfit.TenTo40Units.Value, "%"),
            new ("Прибуток (ЗІЗ) понад 40 шт", addRef.PpeProfit.Above40Units.Value, "%"),
        };

        return Result.Success(result);
    }
}
