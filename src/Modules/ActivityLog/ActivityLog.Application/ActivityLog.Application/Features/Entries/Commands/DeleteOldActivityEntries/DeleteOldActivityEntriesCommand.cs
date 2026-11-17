namespace ActivityLog.Application.Features.Entries.Commands.DeleteOldActivityEntries;

/// <summary>Deletes «Журнал дій» entries older than <see cref="DeleteOldActivityEntriesHandler.KeepYears"/>; answers how many.</summary>
public sealed record DeleteOldActivityEntriesCommand : ICommand<Result<int>>;
