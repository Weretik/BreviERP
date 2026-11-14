using Media.Api.Endpoints.Files.DeleteMediaFile;
using Media.Api.Endpoints.Files.GetMediaFile;
using Media.Api.Endpoints.Files.MoveMediaFiles;
using Media.Api.Endpoints.Files.SearchMediaFiles;
using Media.Api.Endpoints.Files.UpdateMediaFile;
using Media.Api.Endpoints.Files.UploadMediaFile;

namespace Media.Api.Endpoints.Files;

/// <summary>Photos of «Фото»: search, card with usages, upload, rename, move, delete.</summary>
internal static class FilesEndpoints
{
    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/media/files").RequireAuthorization(MediaAccess.Policy).WithTags("MediaFiles");
        SearchMediaFilesEndpoint.Map(group);
        GetMediaFileEndpoint.Map(group);
        UploadMediaFileEndpoint.Map(group);
        UpdateMediaFileEndpoint.Map(group);
        MoveMediaFilesEndpoint.Map(group);
        DeleteMediaFileEndpoint.Map(group);
        return app;
    }
}
