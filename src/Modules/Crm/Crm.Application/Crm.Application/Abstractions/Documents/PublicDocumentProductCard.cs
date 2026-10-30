namespace Crm.Application.Abstractions.Documents;

/// <summary>Allowlisted offer fields. ProductUrl is validated by the HTML writer before use.</summary>
public sealed record PublicDocumentProductCard(
    string ModelName,
    string FabricName,
    string FabricComposition,
    string FabricDensity,
    string ProductUrl,
    decimal LargeWholesale);
