namespace Crm.Application.Documents.EmailTemplates;
public sealed record EmailTemplateDetail(Guid VersionId, int Number, string Name, string Subject, string Body, DateTimeOffset CreatedAt);
