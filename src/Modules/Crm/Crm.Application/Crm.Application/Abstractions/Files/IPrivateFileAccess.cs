using Crm.Application.Customers.Access;
namespace Crm.Application.Abstractions.Files;
// Implement against authoritative CRM ownership; never trust a client supplied owner or file ID.
public interface IPrivateFileAccess
{
    ValueTask<bool> CanAccessConfigurationAsync(CrmActor actor, Guid configurationId, CancellationToken cancellationToken);
}
