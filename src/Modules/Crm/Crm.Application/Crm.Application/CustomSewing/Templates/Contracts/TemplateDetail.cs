using Crm.Domain.CustomSewing.Templates;

namespace Crm.Application.CustomSewing.Templates.Contracts;

public sealed record TemplateDetail(Guid PublicId, int OwnerId, long Revision, bool Archived,
    string Kind, Guid VersionId, int VersionNumber, string Introduction, TemplateSelections Selections,
    DateTimeOffset UpdatedAt, bool CanEdit);
