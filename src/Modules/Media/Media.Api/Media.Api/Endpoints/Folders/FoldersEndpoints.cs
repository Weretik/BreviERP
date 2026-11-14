using Media.Api.Endpoints.Folders.CreateMediaFolder;
using Media.Api.Endpoints.Folders.DeleteMediaFolder;
using Media.Api.Endpoints.Folders.GetMediaFolders;
using Media.Api.Endpoints.Folders.UpdateMediaFolder;

namespace Media.Api.Endpoints.Folders;

/// <summary>Folders of «Фото»: tree, create, rename or move, delete an empty one.</summary>
internal static class FoldersEndpoints
{
    public static IEndpointRouteBuilder MapFoldersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/media/folders").RequireAuthorization(MediaAccess.Policy).WithTags("MediaFolders");
        GetMediaFoldersEndpoint.Map(group);
        CreateMediaFolderEndpoint.Map(group);
        UpdateMediaFolderEndpoint.Map(group);
        DeleteMediaFolderEndpoint.Map(group);
        return app;
    }
}
