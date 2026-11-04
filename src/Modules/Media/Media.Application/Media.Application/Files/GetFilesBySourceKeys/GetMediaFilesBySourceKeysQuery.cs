namespace Media.Application.Files.GetFilesBySourceKeys;

/// <summary>Already imported photos by their source key (for example «old-site:catalog/a.jpg»); unknown keys are left out.</summary>
public sealed record GetMediaFilesBySourceKeysQuery(IReadOnlyCollection<string> SourceKeys)
    : IQuery<Result<IReadOnlyDictionary<string, int>>>;
