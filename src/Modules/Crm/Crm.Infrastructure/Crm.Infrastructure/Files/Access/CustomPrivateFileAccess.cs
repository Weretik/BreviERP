using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Access;

public sealed class CustomPrivateFileAccess(CrmDbContext db) : IPrivateFileAccess
{
    public async ValueTask<bool> CanAccessConfigurationAsync(CrmActor actor, Guid configurationId, CancellationToken ct)
        => actor.UserId > 0 && await db.CustomDrafts.Where(CustomAccess.For(actor)).AnyAsync(x => x.PublicId == configurationId, ct);
}
