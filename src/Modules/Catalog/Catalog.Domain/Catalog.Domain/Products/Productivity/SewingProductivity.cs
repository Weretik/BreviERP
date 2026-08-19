namespace Catalog.Domain.Products.Productivity;

public static class SewingProductivity
{
    public static decimal? CalculatePiecesPerShift(IEnumerable<decimal> operationMinutes)
    {
        var totalMinutes = operationMinutes.Sum();
        return totalMinutes <= 0 ? null : 480m / (totalMinutes * 1.25m);
    }
}
