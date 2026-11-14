namespace Media.Application.Features.Folders.Shared;

/// <param name="FileCount">Photos directly in this folder, without subfolders.</param>
public sealed record MediaFolderResponse(int Id, string Name, int? ParentId, int FileCount);

public sealed record MediaFolderInput(string Name, int? ParentId);
