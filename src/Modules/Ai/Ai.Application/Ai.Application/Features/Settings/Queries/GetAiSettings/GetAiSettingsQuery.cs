using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Queries.GetAiSettings;

public sealed record GetAiSettingsQuery : IQuery<Result<AiSettingsResponse>>;
