namespace Production.Application.Features.Orders.Shared.Responses;

/// <param name="Status">NotSent, Sending, Sent or Failed.</param>
/// <param name="Ready">The workshop group is set and sending is on, so «Надіслати в цех» can work.</param>
public sealed record ProductionTelegramResponse(
    string Status, DateTimeOffset? SentAt, string? SentByName, string? Error, bool Ready);
