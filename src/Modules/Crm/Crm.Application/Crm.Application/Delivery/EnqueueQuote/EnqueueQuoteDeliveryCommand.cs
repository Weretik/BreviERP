namespace Crm.Application.Delivery.EnqueueQuote;

public sealed record EnqueueQuoteDeliveryCommand(Guid QuoteId, Guid RequestId, long ExpectedRevision,
    Guid VersionId, Guid ArtifactId, Guid EmailTemplateVersionId, string RecipientAddress, string ManagerName);
