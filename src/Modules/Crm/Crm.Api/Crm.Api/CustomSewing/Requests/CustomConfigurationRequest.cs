using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CustomConfigurationRequest(string Name, string ProductType, string Season,
    string Description, string ClientComment, string InternalNote,
    IReadOnlyList<CustomMaterialRequest> Fabrics, IReadOnlyList<CustomMaterialRequest> Accessories,
    IReadOnlyList<CustomOperationRequest> Operations, IReadOnlyList<CustomApplicationRequest> Applications,
    decimal Delivery, decimal Packaging)
{
    public Crm.Domain.CustomSewing.Composition.CustomConfiguration ToDomain() => new(Name, ProductType, Season,
        Description, ClientComment, InternalNote,
        Fabrics?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomMaterial(x.ReferenceId, x.Quantity, x.Unit)).ToArray()!,
        Accessories?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomMaterial(x.ReferenceId, x.Quantity, x.Unit)).ToArray()!,
        Operations?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomOperation(x.ReferenceId, x.Quantity, x.ManualMinutes)).ToArray()!,
        Applications?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomApplication(x.Kind, x.Location,
            x.ArtworkFileId, x.Cost, x.SaleAmount, x.SaleApproved)).ToArray()!, Delivery, Packaging);
}
