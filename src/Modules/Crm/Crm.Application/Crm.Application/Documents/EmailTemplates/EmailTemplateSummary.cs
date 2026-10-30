namespace Crm.Application.Documents.EmailTemplates;
public sealed record EmailTemplateSummary(Guid VersionId, int Number, string Name, string Subject, DateTimeOffset CreatedAt);
