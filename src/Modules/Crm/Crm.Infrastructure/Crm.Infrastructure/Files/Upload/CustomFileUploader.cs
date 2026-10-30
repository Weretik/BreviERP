using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Storage;
using Crm.Application.Files.Upload;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Persistence;

namespace Crm.Infrastructure.Files.Upload;

public sealed class CustomFileUploader(CrmDbContext db, IPrivateFileStore bytes, IReservedPrivateFileStore reserved,
    PrivateUploadOptions options, TimeProvider? clock = null) : ICustomFileUploader
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;

    public async Task<Result<PrivateFileView>> UploadAsync(UploadPrivateFileCommand command, CancellationToken ct)
    {
        if (command.Actor.UserId <= 0 || command.DraftId == Guid.Empty || command.RequestId == Guid.Empty || command.Content is null)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        var reservations = new PrivateUploadReservationStore(db, time, options);
        if (!await reservations.AccessibleAsync(command.DraftId, command.Actor, ct)) return Result.NotFound();
        var prepared = await reserved.PrepareAsync(ct);
        if (!prepared.IsSuccess) return Result.Error("private_file_storage_unavailable");
        if (options.TemporaryLifetime < TimeSpan.FromMinutes(1) || options.TemporaryLifetime > TimeSpan.FromDays(7))
            return Result.Error("private_file_storage_unavailable");
        var input = await ValidatedUploadBody.ReadAsync(command.DraftId, command.Content, command.ContentType, ct);
        if (!input.IsSuccess) return Failure(input);
        var reservation = await reservations.ReserveAsync(command, input.Value, ct);
        if (!reservation.IsSuccess) return Failure(reservation);
        var file = reservation.Value;
        if (file.State == CustomFileState.Reserved)
        {
            using var content = new MemoryStream(input.Value.Bytes);
            var written = await reserved.WriteReservedAsync(command.DraftId, file.PublicId, content,
                file.ContentType, file.ExpiresAt, ct);
            if (!written.IsSuccess) return Failure(written);
        }
        var verified = await bytes.ReadAsync(command.DraftId, file.PublicId, ct);
        if (!verified.IsSuccess) return Failure(verified);
        if (verified.Value.File.Length != file.Length || verified.Value.File.Sha256 != file.Sha256
            || verified.Value.File.ContentType != file.ContentType)
            return Result.Invalid(new ValidationError("file", "private_file_verification_failed"));
        var completed = await reservations.CompleteAsync(command.DraftId, file.PublicId, command.Actor, ct);
        if (!completed.IsSuccess) return Failure(completed);
        file = completed.Value;
        return Result.Success(new PrivateFileView(file.PublicId, command.DraftId, file.ContentType, file.Length,
            file.Sha256, file.State.ToString(), file.ExpiresAt, false, false, false));
    }

    private static Result<PrivateFileView> Failure<T>(Result<T> result) => result.Status switch
    {
        ResultStatus.NotFound => Result.NotFound(),
        ResultStatus.Invalid => Result.Invalid(result.ValidationErrors),
        ResultStatus.Conflict => Result.Conflict(result.Errors.ToArray()),
        _ => Result.Error("private_file_storage_unavailable")
    };
}
