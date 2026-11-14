using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiSettings;

/// <summary>Models, prices, on/off and the monthly limit. <paramref name="Version"/> is the version the admin saw.</summary>
public sealed record UpdateAiSettingsCommand(
    bool Enabled,
    string FastModel,
    decimal FastInputPrice,
    decimal FastOutputPrice,
    string StrongModel,
    decimal StrongInputPrice,
    decimal StrongOutputPrice,
    decimal MonthlyLimitUsd,
    int Version,
    string By) : ICommand<Result<AiSettingsResponse>>;
