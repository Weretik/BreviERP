using System.Text.Json.Serialization;
using Crm.Api.CustomSewing.Requests;
using Crm.Domain.CustomSewing.Templates;
namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record TemplateSelectionsRequest([property: JsonRequired] string Name, [property: JsonRequired] string ProductType,
    [property: JsonRequired] string Season, [property: JsonRequired] string Description, [property: JsonRequired] string ClientComment,
    [property: JsonRequired] IReadOnlyList<CustomMaterialRequest> Fabrics,
    [property: JsonRequired] IReadOnlyList<CustomMaterialRequest> Accessories,
    [property: JsonRequired] IReadOnlyList<CustomOperationRequest> Operations,
    [property: JsonRequired] IReadOnlyList<TemplateApplicationRequest> Applications)
{
    public TemplateSelections ToDomain() => new(Name, ProductType, Season, Description, ClientComment,
        Fabrics?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomMaterial(x.ReferenceId, x.Quantity, x.Unit)).ToArray()!,
        Accessories?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomMaterial(x.ReferenceId, x.Quantity, x.Unit)).ToArray()!,
        Operations?.Select(x => x is null ? null! : new Crm.Domain.CustomSewing.Composition.CustomOperation(x.ReferenceId, x.Quantity, x.ManualMinutes)).ToArray()!,
        Applications?.Select(x => x is null ? null! : new TemplateApplication(x.Kind, x.Location)).ToArray()!);
}
