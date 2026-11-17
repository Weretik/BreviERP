namespace Reference.Contracts.Pricing;

/// <summary>
/// Told in the same request after a save adds a pricing input version (a new or changed fabric, accessory or
/// operation price) or deletes one of those rows, so a module that prices from them (Catalog) republishes its prices
/// once the request has finished (CAT-012, ADR-0019). A rename or another change without a new version tells nothing.
/// </summary>
public interface IPricingInputSubscriber
{
    void PricingInputsChanged();
}
