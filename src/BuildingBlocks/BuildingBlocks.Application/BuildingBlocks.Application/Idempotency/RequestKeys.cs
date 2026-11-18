using System.Security.Cryptography;
using BuildingBlocks.Domain;

namespace BuildingBlocks.Application.Idempotency;

/// <summary>
/// Builds the <see cref="RequestKey"/> of a create that takes a <c>requestId</c> (idempotency rules 2): the hash covers
/// the whole command without the id, so the same id with other data answers <see cref="Conflict{T}"/> (409) instead of
/// the first row.
/// </summary>
public static class RequestKeys
{
    public const string ConflictMessage = "Цей запит уже виконано з іншими даними. Оновіть сторінку.";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>The key, or null when the client sent no id. Pass the command with its request id cleared.</summary>
    public static RequestKey? Of(Guid? requestId, object request)
        => requestId is { } id ? RequestKey.Create(id, Hash(request)) : null;

    public static string Hash(object request)
        => Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), Json)));

    public static Result<T> Conflict<T>() => Result<T>.Conflict(ConflictMessage);

    /// <summary>Null when nothing is saved under the key's id; else whether it was saved from the same data.</summary>
    public static bool? Same(RequestKey key, Saved? saved) => saved is null ? null : key.Matches(saved.Hash);

    /// <summary>The hash a row saved under a request id holds; project to it with <c>Select</c>.</summary>
    public sealed record Saved(string? Hash);
}
