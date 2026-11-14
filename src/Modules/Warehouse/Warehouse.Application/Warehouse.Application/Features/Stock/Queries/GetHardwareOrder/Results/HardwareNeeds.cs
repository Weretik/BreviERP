using System.Text;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder.Results;

/// <summary>Which codes are physical accessories (TZ §14.1) and how the need adds up (§14.2).</summary>
public static class HardwareNeeds
{
    public const int Decimals = 6;

    public const string NoComposition = "Модель не має складу (комплектації): кількість невідома.";
    public const string NoNorm = "Немає норми на виріб: кількість невідома.";
    public const string NoProducts = "Немає кількості виробів: кількість невідома.";
    public const string NoCode = "Фурнітура без коду: кількість не порахована.";
    public const string UnknownCode = "Не визначено, чи це фурнітура: кількість не порахована.";

    /// <summary>Textile components (the fabric scenario) and the service «Інші витрати».</summary>
    private static readonly HashSet<int> Excluded = [21, 22, 23, 24, 26, 29, 32];

    private static readonly HashSet<int> Included =
        [.. Enumerable.Range(1, 20), 25, 27, 31, 33, .. Enumerable.Range(35, 5)];

    /// <summary>Included (FUR-001…020, 025, 027, 031, 033, 035…039), excluded, or not known.</summary>
    public static bool? IsHardware(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();
        if (!normalized.StartsWith("FUR-", StringComparison.Ordinal) ||
            !int.TryParse(normalized[4..], System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var number) ||
            normalized.Length != 7)
            return null;
        return Included.Contains(number) ? true : Excluded.Contains(number) ? false : null;
    }

    public static (IReadOnlyList<HardwareNeedResponse> Needs, IReadOnlyList<HardwareIncompleteResponse> Incomplete)
        Calculate(IReadOnlyList<WarehouseAssemblyItem> items)
    {
        var shares = new List<(string Code, string? Name, string? Note, string Unit, HardwareNeedOrderResponse Share)>();
        var incomplete = new List<HardwareIncompleteResponse>();
        foreach (var item in items.OrderBy(x => x.OrderCreatedAt).ThenBy(x => x.OrderNumber).ThenBy(x => x.ItemId))
        {
            if (item.Lines is null)
            {
                incomplete.Add(Incomplete(item, null, null, NoComposition));
                continue;
            }

            foreach (var line in item.Lines)
            {
                var isAccessory = line.Kind == "Accessory";
                if (string.IsNullOrWhiteSpace(line.Code))
                {
                    if (isAccessory) incomplete.Add(Incomplete(item, null, line.Name, NoCode));
                    continue;
                }

                var code = line.Code.Trim().ToUpperInvariant();
                if (!code.StartsWith("FUR-", StringComparison.Ordinal) && !isAccessory) continue;
                switch (IsHardware(code))
                {
                    case false:
                        continue;
                    case null:
                        incomplete.Add(Incomplete(item, code, line.Name, UnknownCode));
                        continue;
                }

                if (line.Norm <= 0)
                    incomplete.Add(Incomplete(item, code, line.Name, NoNorm));
                else if (item.Quantity <= 0)
                    incomplete.Add(Incomplete(item, code, line.Name, NoProducts));
                else
                    shares.Add((code, Clean(line.Name), Clean(line.Comment), line.Unit,
                        new HardwareNeedOrderResponse(item.OrderId, item.OrderNumber, item.ClientName, item.ItemId,
                            item.ModelName, line.Norm, item.Quantity,
                            decimal.Round(line.Norm * item.Quantity, Decimals, MidpointRounding.AwayFromZero),
                            item.DueDate)));
            }
        }

        var needs = shares.GroupBy(x => (x.Code, x.Name, x.Note, x.Unit))
            .Select(g => new HardwareNeedResponse(g.Key.Code, g.Key.Name ?? g.Key.Code, g.Key.Name is not null,
                g.Key.Note, g.Key.Unit,
                decimal.Round(g.Sum(x => x.Share.Quantity), Decimals, MidpointRounding.AwayFromZero),
                g.Select(x => x.Share).ToList(), g.Min(x => x.Share.DueDate)))
            .OrderBy(x => x.Code, StringComparer.Ordinal).ThenBy(x => x.Name, StringComparer.CurrentCulture)
            .ThenBy(x => x.Note, StringComparer.CurrentCulture).ThenBy(x => x.Unit, StringComparer.Ordinal)
            .ToList();
        return (needs, incomplete);
    }

    /// <summary>NFC with spaces squeezed, so the same description groups together; null when empty.</summary>
    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Normalize(NormalizationForm.FormC).Replace(' ', ' ');
        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static HardwareIncompleteResponse Incomplete(WarehouseAssemblyItem item, string? code, string? name,
        string reason) => new(item.OrderId, item.OrderNumber, item.ClientName, item.ItemId, item.ModelName, code, name,
        reason);
}
