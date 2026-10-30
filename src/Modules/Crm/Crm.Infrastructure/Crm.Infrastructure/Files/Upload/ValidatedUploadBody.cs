using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using Crm.Application.Files.Validation;

namespace Crm.Infrastructure.Files.Upload;

internal sealed record ValidatedUploadBody(byte[] Bytes, string ContentType, string Sha256, string IntentHash)
{
    internal static async Task<Result<ValidatedUploadBody>> ReadAsync(Guid draftId, Stream content, string type, CancellationToken ct)
    {
        if (type is not ("application/pdf" or "image/jpeg" or "image/png"))
            return Result.Invalid(new ValidationError("file", "unsupported_file_type"));
        using var destination = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = await content.ReadAsync(buffer, ct);
            if (read == 0) break;
            if (destination.Length + read > PrivateFilePolicy.MaximumBytes)
                return Result.Invalid(new ValidationError("file", "file_too_large"));
            await destination.WriteAsync(buffer.AsMemory(0, read), ct);
        }
        var bytes = destination.ToArray();
        if (!PrivateFilePolicy.MatchesAllowedContent(bytes, type))
            return Result.Invalid(new ValidationError("file", "unsupported_file_type"));
        var sha = Convert.ToHexString(SHA256.HashData(bytes));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{draftId:N}:{type}:{bytes.Length}:{sha}")));
        return Result.Success(new ValidatedUploadBody(bytes, type, sha, hash));
    }
}
