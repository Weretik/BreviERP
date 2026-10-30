using Ardalis.Result;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;

namespace Crm.Application.CustomSewing.Production.Projection;

internal static class ProductionOperationProjection
{
    internal static Result<IReadOnlyList<ProductionOperation>> Build(
        IReadOnlyList<CustomOperation> operations, IReadOnlyList<CustomReferenceBasis> references)
    {
        var rows = new List<ProductionOperation>(operations.Count);
        foreach (var operation in operations)
        {
            // This is the exact key/unit contract written by the saved sewing pricing adapter.
            var matches = references.Where(x => x is not null && x.Key == $"Operation:{operation.ReferenceId}").ToArray();
            if (matches.Length != 1 || matches[0].VersionId <= 0 || matches[0].Unit != "Minutes" ||
                (operation.ManualMinutes ?? matches[0].Value) is not > 0)
                return Result<IReadOnlyList<ProductionOperation>>.Invalid(
                    new ValidationError("operations", "production_operation_basis_missing_or_ambiguous"));
            var basis = matches[0];
            rows.Add(new ProductionOperation(operation.ReferenceId, operation.Quantity,
                operation.ManualMinutes, basis.Value, (operation.ManualMinutes ?? basis.Value)!.Value, basis.VersionId));
        }
        return Result.Success<IReadOnlyList<ProductionOperation>>(rows.AsReadOnly());
    }
}
