namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Work for the outside world (or another module) saved in the same <c>SaveChanges</c> as the change that causes it
/// (module rules «Transactional outbox», PLT-003). Each module keeps these rows in its own schema.
/// </summary>
public sealed class OutboxMessage
{
    public const int TypeMaxLength = 100;
    public const int ErrorMaxLength = 2000;

    /// <summary>After this many failures the row stays with its last error and is not taken again.</summary>
    public const int MaxAttempts = 10;

    private const int MaxDelayMinutes = 60;

    private OutboxMessage() { }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public int Attempts { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? LastError { get; private set; }

    public bool GaveUp => ProcessedAt is null && Attempts >= MaxAttempts;

    /// <param name="type">A stable name such as <c>crm.site-request.notify</c>; the handler with that type runs it.</param>
    /// <param name="payload">What the handler needs, serialized as JSON (ids, not copies of the data).</param>
    public static OutboxMessage Create<TPayload>(string type, TPayload payload, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(type) || type.Length > TypeMaxLength)
            throw new ArgumentException($"An outbox type is 1 to {TypeMaxLength} characters.", nameof(type));
        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(now),
            Type = type,
            Payload = JsonSerializer.Serialize(payload, Json),
            CreatedAt = now,
            NextAttemptAt = now,
        };
    }

    public static TPayload Read<TPayload>(string payload) =>
        JsonSerializer.Deserialize<TPayload>(payload, Json)
        ?? throw new InvalidOperationException("The outbox payload is empty.");

    public void Succeed(DateTimeOffset now)
    {
        ProcessedAt = now;
        LastError = null;
    }

    /// <summary>The next try waits 1, 2, 4 … minutes, at most an hour.</summary>
    public void Fail(string error, DateTimeOffset now)
    {
        Attempts++;
        LastError = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
        NextAttemptAt = now.AddMinutes(Math.Min(MaxDelayMinutes, 1 << Math.Min(Attempts - 1, 6)));
    }

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
}
