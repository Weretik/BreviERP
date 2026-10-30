namespace Crm.Application.Documents.EmailTemplates;
public interface IEmailTemplateVersionReader
{
    Task<bool> ExistsAsync(Guid versionId, CancellationToken cancellationToken);
}
