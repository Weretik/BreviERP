using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.Features.FabricNorms.Commands.DeleteFabricNorm;

/// <summary>«Очистити норму»: the model stays in the list without a norm.</summary>
public sealed record DeleteFabricNormCommand(string ModelCode) : ICommand<Result>;
