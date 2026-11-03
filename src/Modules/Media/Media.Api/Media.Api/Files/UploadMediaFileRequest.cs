using Microsoft.AspNetCore.Http;

namespace Media.Api.Files;

/// <param name="FolderId">Target folder; empty for the library root.</param>
public sealed record UploadMediaFileRequest(IFormFile File, int? FolderId);
