using Purchasing.Domain.Messages;

namespace Purchasing.Domain.Ingestion;

/// <summary>A non-empty sheet row with its ids, raw text, links, problems and message fields.</summary>
public sealed record NormalizedSourceRow(
    string SourceId,
    string MessageId,
    string TelegramKey,
    string Uid,
    int RowNumber,
    string RawJson,
    IReadOnlyList<string> Links,
    IReadOnlyList<string> Issues,
    MessageSource Message);
