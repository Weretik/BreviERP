using Ai.Application.Abstractions;
using Ai.Application.Contracts;
using Ai.Domain.Settings;

namespace Ai.Application.Settings;

public sealed record GetAiSettingsQuery : IQuery<Result<AiSettingsResponse>>;

public sealed class GetAiSettingsQueryHandler(IAiDbContext db, AiSettingsStore store)
    : IQueryHandler<GetAiSettingsQuery, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(GetAiSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == AiSettings.OpenAi, cancellationToken)
            ?? AiSettings.CreateDefault();
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
