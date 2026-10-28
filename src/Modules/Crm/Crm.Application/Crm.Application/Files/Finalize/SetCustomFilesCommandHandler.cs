using Ardalis.Result;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.Persistence;
using Crm.Application.Files.Validation;
using Mediator;

namespace Crm.Application.Files.Finalize;

public sealed class SetCustomFilesCommandHandler(ICustomMutationStore mutations, IFinalizedCustomFilesWriter files)
    : ICommandHandler<SetCustomFilesCommand, Result<CustomDetail>>
{
    public async ValueTask<Result<CustomDetail>> Handle(SetCustomFilesCommand command, CancellationToken ct)
    {
        if (command.DraftId == Guid.Empty || command.RequestId == Guid.Empty || command.ExpectedRevision <= 0
            || command.Actor.UserId <= 0 || command.Files is null || command.Files.Count > PrivateFilePolicy.MaximumFilesPerConfiguration
            || command.Files.Any(x => x is null || x.FileId == Guid.Empty)
            || command.Files.Select(x => x.FileId).Distinct().Count() != command.Files.Count)
            return Result.Invalid(new ValidationError("files", "invalid_private_file_selection"));
        return await mutations.ExecuteAsync("SetCustomFiles", command.RequestId, command.Actor, command, async (session, token) =>
        {
            var draft = await session.FindDraftAsync(command.DraftId, command.Actor, token);
            if (draft is null) return Result.NotFound();
            if (draft.Revision != command.ExpectedRevision || draft.Quote?.Archived == true)
                return Result.Conflict("revision_conflict");
            await files.ReplaceAsync(draft, command.Files, command.Actor, token);
            draft.Revise(command.ExpectedRevision, draft.Configuration);
            await session.FlushAsync(token);
            return Result.Success(await session.DetailAsync(draft, token));
        }, ct);
    }
}
