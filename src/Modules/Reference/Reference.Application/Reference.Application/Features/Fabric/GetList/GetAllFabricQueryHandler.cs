using Crm.Application.Contracts;
using Crm.Domain.Entities;
using Crm.Domain.ValueObjects;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Fabric.GetList.DTOs;
using Reference.Application.Features.Fabric.GetList.Specifications;

namespace Reference.Application.Features.Fabric.GetList;

public class GetAllFabricQueryHandler(
    IReferenceReadRepository<Domain.Entities.Fabric> referenceRepository, ICrmReadRepository<Counterparty> crmRepository)
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
