using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Contracts.Pricing.Ppe;
public sealed record PpeRuleVersion(Guid Id, PpeRuleValues Values, bool IsApproved, string? ApprovedBy, DateTimeOffset? ApprovedAt, DateTimeOffset CreatedAt);
