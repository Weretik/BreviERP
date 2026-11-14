using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing.Ppe.Versions;
public sealed record PpeOfferVersion(Guid Id, int ProductId, PpeOfferValues Values, bool IsApproved, string? ApprovedBy, DateTimeOffset? ApprovedAt, DateTimeOffset CreatedAt);
