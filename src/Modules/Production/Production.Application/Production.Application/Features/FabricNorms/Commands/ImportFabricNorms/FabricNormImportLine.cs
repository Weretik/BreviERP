namespace Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;

/// <param name="Action">created, updated, unchanged or error.</param>
public sealed record FabricNormImportLine(string ModelCode, string? ModelName, string Action, string? Error);
