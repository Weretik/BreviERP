using Crm.Application.CustomSewing.Contracts;
namespace Crm.Application.CustomSewing.ApplyTemplate;

public sealed record AppliedTemplate(CustomDetail Draft, Guid TemplateVersionId, string Introduction);
