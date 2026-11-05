namespace Purchasing.Domain.Ingestion;

/// <summary>The exact A:N header of the source sheet «финансы», in order; the sync refuses any other header.</summary>
public static class PurchaseSheetColumns
{
    public static readonly IReadOnlyList<string> Header =
    [
        "UID", "ChatName", "ChatId", "MessageId", "DateTime", "FromUser", "FromUserId",
        "Text", "MessageType", "FileLinks", "FileCount", "Status", "StatusDate", "FromUserStatus",
    ];

    public const int Uid = 0;
    public const int ChatName = 1;
    public const int ChatId = 2;
    public const int MessageId = 3;
    public const int DateTime = 4;
    public const int FromUser = 5;
    public const int FromUserId = 6;
    public const int Text = 7;
    public const int MessageType = 8;
    public const int FileLinks = 9;
    public const int FileCount = 10;
    public const int Status = 11;
    public const int StatusDate = 12;
    public const int FromUserStatus = 13;

    /// <summary>A row longer than this is not a row of this sheet.</summary>
    public const int MaxRowLength = 30;

    /// <summary>The sheet grid may have at most this many rows.</summary>
    public const int MaxGridRows = 20_000;
}
