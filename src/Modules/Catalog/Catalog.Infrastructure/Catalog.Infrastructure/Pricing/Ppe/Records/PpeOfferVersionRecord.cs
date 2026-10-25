using System.Text.Json;
using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Infrastructure.Pricing.Ppe.Records;
public sealed class PpeOfferVersionRecord
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public Guid RequestId { get; set; }
    public string RequestHash { get; set; } = string.Empty;
    public string ValuesJson { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Catalog.Domain.Products.ValueObjects.ProductId ProductId { get; set; }
    public PpeOfferValues Values() => JsonSerializer.Deserialize<PpeOfferValues>(ValuesJson)!;
    public PpeOfferVersion ToVersion() => new(PublicId, ProductId.Value, Values(), ApprovedBy is not null, ApprovedBy, ApprovedAt, CreatedAt);
}
