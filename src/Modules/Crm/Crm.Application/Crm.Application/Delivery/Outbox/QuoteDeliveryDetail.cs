namespace Crm.Application.Delivery.Outbox;

public sealed record QuoteDeliveryDetail(Guid DeliveryId, Guid QuoteId, Guid VersionId, Guid ArtifactId,
    Guid EmailTemplateVersionId, string RecipientAddress, string Status, int Attempt,
    string? ErrorCode, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? ConfirmedAt);
