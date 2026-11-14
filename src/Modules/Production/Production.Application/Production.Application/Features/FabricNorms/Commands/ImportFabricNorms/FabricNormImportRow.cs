using Production.Application.Features.FabricNorms.Shared;
namespace Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;

public sealed record FabricNormImportRow(string ModelCode, FabricNormInput Norm);
