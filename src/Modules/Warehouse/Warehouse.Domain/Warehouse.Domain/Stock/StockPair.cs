using System.Security.Cryptography;
using System.Text;

namespace Warehouse.Domain.Stock;

/// <summary>Why a pair operation was refused.</summary>
public enum StockPairRefusal
{
    /// <summary>«Початковий залишок» of a fabric and colour that the stock already holds under any code of its group.</summary>
    AlreadyOpened = 1,

    /// <summary>«Уточнити» would create or deepen a negative balance.</summary>
    DeepensShortage = 2,

    /// <summary>The target equals the balance.</summary>
    NothingToAdjust = 3,

    /// <summary>The pair has no stock line to spend from.</summary>
    NotHeld = 4,

    /// <summary>A line would leave the allowed range of metres.</summary>
    OutOfRange = 5
}

/// <summary>
/// One fabric (canonical code of <see cref="FabricAliases"/>) in one colour: every stock line of the group with that
/// colour key. Openings, receipts, issues without an order and «Уточнити» work on the pair; each movement still
/// belongs to one line, which keeps its own code.
/// </summary>
public sealed class StockPair
{
    private readonly List<FabricStock> _lines;

    /// <param name="lines">Lines of the group whose colour key is <paramref name="colorKey"/>.</param>
    public StockPair(string code, string colorKey, IEnumerable<FabricStock> lines)
    {
        Code = FabricAliases.Canonical(code);
        ColorKey = colorKey;
        _lines = lines.Where(x => FabricAliases.Canonical(x.FabricCode) == Code && x.ColorKey == colorKey)
            .OrderBy(x => x.FabricCode == Code ? 0 : 1).ThenBy(x => x.FabricCode, StringComparer.Ordinal)
            .ThenBy(x => x.Id).ToList();
    }

    public string Code { get; }
    public string ColorKey { get; }

    /// <summary>Lines in spending order: the canonical code first, then by code and id.</summary>
    public IReadOnlyList<FabricStock> Lines => _lines;

    public decimal Meters => _lines.Sum(x => x.Meters);

    /// <summary>
    /// Changes whenever any line of the pair changes: alias revision, code, colour and every line's id, code, balance
    /// and version. A form sends it back so a change in between is a conflict.
    /// </summary>
    public string Version
    {
        get
        {
            var text = string.Join('|', new[] { FabricAliases.Revision, Code, ColorKey }.Concat(_lines.OrderBy(x => x.Id)
                .Select(x => FormattableString.Invariant($"{x.Id}:{x.FabricCode}:{x.Meters:0.00}:{x.Version}"))));
            return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
        }
    }

    /// <summary>
    /// «Початковий залишок»: allowed once per pair, zero included. The new line keeps the exact code picked.
    /// </summary>
    public StockPairResult Open(StockFabric fabric, string color, decimal meters, FabricMovementDetails details,
        string actorName, DateTimeOffset now)
    {
        if (_lines.Count > 0) return StockPairResult.Refused(StockPairRefusal.AlreadyOpened);
        if (!FabricStock.IsValidMeters(meters) || meters < 0) return StockPairResult.Refused(StockPairRefusal.OutOfRange);
        var line = new FabricStock(fabric, color, now);
        _lines.Add(line);
        return StockPairResult.Done(line, line.Move(FabricMovementKind.Opening, meters, details, actorName, now));
    }

    /// <summary>«Прихід» to the line of the exact code picked; a new line when the pair never held that code.</summary>
    public StockPairResult Receive(StockFabric fabric, string color, decimal meters, FabricMovementDetails details,
        string actorName, DateTimeOffset now)
    {
        var code = FabricAliases.Normalize(fabric.Code);
        var line = _lines.FirstOrDefault(x => x.FabricCode == code);
        if (line is not null && !FabricStock.IsValidMeters(line.Meters + meters))
            return StockPairResult.Refused(StockPairRefusal.OutOfRange);
        if (line is null)
        {
            line = new FabricStock(fabric, color, now);
            _lines.Add(line);
        }

        return StockPairResult.Done(line, line.Move(FabricMovementKind.Receipt, meters, details, actorName, now));
    }

    /// <summary>
    /// «Видача»: positive parts of the lines are spent in order; what is left is taken from the first line even below
    /// zero, because the fabric really left the shelf.
    /// </summary>
    public StockPairResult Issue(decimal meters, FabricMovementDetails details, string actorName, DateTimeOffset now) =>
        Spend(FabricMovementKind.Issue, meters, details, actorName, now);

    /// <summary>
    /// «Уточнити»: the counted total of the pair. It may not end below min(0, current total); a decrease is spent like
    /// an issue, an increase goes to the first line.
    /// </summary>
    public StockPairResult Adjust(decimal target, FabricMovementDetails details, string actorName, DateTimeOffset now)
    {
        if (_lines.Count == 0) return StockPairResult.Refused(StockPairRefusal.NotHeld);
        var before = Meters;
        if (target == before) return StockPairResult.Refused(StockPairRefusal.NothingToAdjust);
        if (target < Math.Min(0m, before)) return StockPairResult.Refused(StockPairRefusal.DeepensShortage);
        if (target < before) return Spend(FabricMovementKind.Adjustment, before - target, details, actorName, now);
        var anchor = _lines[0];
        if (!FabricStock.IsValidMeters(anchor.Meters + target - before))
            return StockPairResult.Refused(StockPairRefusal.OutOfRange);
        return StockPairResult.Done(anchor,
            anchor.Move(FabricMovementKind.Adjustment, target - before, details, actorName, now));
    }

    private StockPairResult Spend(FabricMovementKind kind, decimal meters, FabricMovementDetails details,
        string actorName, DateTimeOffset now)
    {
        if (_lines.Count == 0) return StockPairResult.Refused(StockPairRefusal.NotHeld);
        var plan = new List<(FabricStock Line, decimal Amount)>();
        var remaining = meters;
        foreach (var line in _lines)
        {
            var amount = Math.Min(remaining, Math.Max(0m, line.Meters));
            if (amount > 0) plan.Add((line, amount));
            remaining -= amount;
            if (remaining == 0) break;
        }

        if (remaining > 0)
        {
            var index = plan.FindIndex(x => ReferenceEquals(x.Line, _lines[0]));
            if (index >= 0) plan[index] = (_lines[0], plan[index].Amount + remaining);
            else plan.Insert(0, (_lines[0], remaining));
        }

        if (plan.Any(x => !FabricStock.IsValidMeters(x.Line.Meters - x.Amount)))
            return StockPairResult.Refused(StockPairRefusal.OutOfRange);
        var movements = plan.Select(x => x.Line.Move(kind, -x.Amount, details, actorName, now)).ToList();
        return new StockPairResult(null, plan.Select(x => x.Line).ToList(), movements);
    }
}

/// <summary>The lines a pair operation touched and their new movements, or why it was refused.</summary>
public sealed record StockPairResult(StockPairRefusal? Refusal, IReadOnlyList<FabricStock> Lines,
    IReadOnlyList<FabricMovement> Movements)
{
    public bool IsDone => Refusal is null;

    public static StockPairResult Refused(StockPairRefusal refusal) => new(refusal, [], []);

    public static StockPairResult Done(FabricStock line, FabricMovement movement) => new(null, [line], [movement]);
}
