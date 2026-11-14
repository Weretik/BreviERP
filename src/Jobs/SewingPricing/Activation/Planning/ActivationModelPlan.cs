using Catalog.Application.Features.Product.Shared.Sewing;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationModelPlan(int ProductId, string? ModelCode, long ExpectedVersion,
    IReadOnlyList<SewingMaterialInput> Materials, IReadOnlyList<SewingOperationInput> Operations,
    IReadOnlyList<ActivationArticlePlan> Articles);
