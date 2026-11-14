using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Queries.GetAiSettings;

public sealed class GetAiSettingsHandler(IAiDbContext db, AiSettingsStore store)
    : IQueryHandler<GetAiSettingsQuery, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(GetAiSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == AiSettings.OpenAi, cancellationToken)
            ?? AiSettings.CreateDefault();
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
