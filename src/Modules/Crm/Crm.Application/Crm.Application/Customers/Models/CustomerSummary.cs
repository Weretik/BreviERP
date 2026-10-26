using Crm.Domain.Customers;

namespace Crm.Application.Customers.Models;

public sealed record CustomerSummary(Guid PublicId, string Kind, string DisplayName, string? LegalName, string? TaxIdentity, int OwnerId, bool Active, long Revision, DateTimeOffset UpdatedAt);
