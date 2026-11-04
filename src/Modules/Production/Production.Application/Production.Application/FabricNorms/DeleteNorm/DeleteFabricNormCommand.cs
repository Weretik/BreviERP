using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.DeleteNorm;

/// <summary>«Очистити норму»: the model stays in the list without a norm.</summary>
public sealed record DeleteFabricNormCommand(string ModelCode) : ICommand<Result>;
