namespace Crm.Domain.Quotes.Sending;

public static class QuoteSendingLimits
{
    public const int EmailMaxLength = 320;
    public const int MaxRecipients = 10;
    public const int RecipientsMaxLength = 2000;
    public const int SubjectMaxLength = 300;
    public const int BodyMaxLength = 10000;
    public const int TokenMaxLength = 4000;
}
