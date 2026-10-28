using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Infrastructure.Files.Cleanup;

internal sealed record TemporaryFileCleanupClaim(CustomFileId Id, CustomDraftId DraftId, Guid Root, Guid File);
