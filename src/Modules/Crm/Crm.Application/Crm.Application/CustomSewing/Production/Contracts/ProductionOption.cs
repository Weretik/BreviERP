namespace Crm.Application.CustomSewing.Production.Contracts;

public sealed record ProductionOption(Guid QuoteVariantPublicId, Guid CustomDraftPublicId,
    Guid CustomVersionPublicId, int CustomVersionNumber, string Name, string ProductType, string Season,
    IReadOnlyList<ProductionMaterial> Fabrics, IReadOnlyList<ProductionMaterial> Accessories,
    IReadOnlyList<ProductionOperation> Operations, IReadOnlyList<ProductionApplication> Applications,
    string FormulaVersion, long SettingsVersionId, ProductionWageBasis WageBasis,
    IReadOnlyList<Guid> ProductionFileIds);
