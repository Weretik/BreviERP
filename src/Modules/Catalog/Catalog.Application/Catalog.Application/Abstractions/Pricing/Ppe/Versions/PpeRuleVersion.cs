using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing.Ppe.Versions;
public sealed record PpeRuleVersion(Guid Id, PpeRuleValues Values, bool IsApproved, string? ApprovedBy, DateTimeOffset? ApprovedAt, DateTimeOffset CreatedAt);
