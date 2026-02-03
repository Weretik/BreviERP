using Application.CRM.Shared.Contracts;
using Domain.CRM.Entities;
using Domain.CRM.ValueObjects;
using Domain.Reference.Entities;
using Reference.Application.Contracts.Persistence;

namespace Application.Reference.GetAllFabric;

public class GetAllFabricQueryHandler(
    IReferenceReadRepository<Fabric> referenceRepository, ICrmReadRepository<Counterparty> crmRepository)
    : IQueryHandler<GetAllFabricQuery, Result<List<FabricRowDTO>>>
{
    public async ValueTask<Result<List<FabricRowDTO>>> Handle(GetAllFabricQuery query,
        CancellationToken cancellationToken)
    {
        var fabrics = await referenceRepository.ListAsync(new AllFabricsSpec(), cancellationToken);

        var supplierIds = fabrics
            .Select(f => CounterpartyId.From(f.CounterpartyId))
            .Distinct()
            .ToList();

        var suppliers = await crmRepository.ListAsync(
            new CounterpartyByIdsSpec(supplierIds), cancellationToken);

        var supplierDictionary = suppliers.ToDictionary(
            c => c.Id.Value,
            c => c.Name);

        var result = fabrics
            .Select(f => new FabricRowDTO(
                Id: f.Id.Value,
                Name: f.Name,
                CounterpartyName: supplierDictionary.GetValueOrDefault(f.CounterpartyId, "—"),
                Price: f.Price.Amount
            ))
            .ToList();

        return Result.Success(result);
    }
}
