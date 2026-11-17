namespace Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

/// <summary>
/// Scoped (one per request or job step) record of saved changes to Sewing pricing inputs: Reference fabrics,
/// accessories and operations, a model's composition and the sewing settings. Catalog marks it when it saves, and Reference
/// tells it through <c>IPricingInputSubscriber</c> (ADR-0019); the prices are published once the request has finished
/// (CAT-012).
/// </summary>
public sealed class PricingInputChanges
{
    private readonly HashSet<int> _productIds = [];
    private bool _all;

    public bool HasChanges => _all || _productIds.Count > 0;

    /// <summary>The changed Sewing models; null when every model is affected.</summary>
    public IReadOnlyCollection<int>? ProductIds => _all ? null : _productIds;

    /// <summary>A shared input changed (a Reference row or the settings): every model may be affected.</summary>
    public void MarkAll() => _all = true;

    public void MarkProduct(int productId)
    {
        if (!_all)
            _productIds.Add(productId);
    }

    public void Clear()
    {
        _all = false;
        _productIds.Clear();
    }
}
