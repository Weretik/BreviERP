using Crm.Application.Imports.History.Contracts;
namespace UnitTests.Crm.Imports;

internal static class CrmHistoryPlannerFixture
{
    internal static string Hash(char c) => new(c, 64);
    internal static readonly string Owner = Hash('a');
    internal static readonly string IdentityHash = Hash('b');
    internal static CrmHistoryRow Row(string id = "client1", string entity = "counterparties", string? kind = null)
        => new(entity, id, Hash('c'), entity == "counterparties" ? ["CLIENT"] : [], [Owner], [], [], kind, []);
    internal static CrmHistoryDryRunRequest Request(params CrmHistoryRow[] rows)
    {
        var tables = rows.GroupBy(x => x.Entity).Select(g => new CrmHistoryTable(g.Key, g.Count(), g.Count(), true, [])).ToArray();
        return new(new("calculator_archive", Hash('d'), Hash('e'), true, true, tables, rows, []),
            new(new Dictionary<string, int> { { Owner, 17 } }, new Dictionary<string, string>(), [], "scope_reviewed", "retention_reviewed"));
    }
}
