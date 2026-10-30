using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Crm.Api.Files.Requests;

public sealed class UploadPrivateFileRequest
{
    public Guid DraftId { get; init; }
    public Guid RequestId { get; init; }
    [Required] public IFormFile File { get; init; } = null!;
}
