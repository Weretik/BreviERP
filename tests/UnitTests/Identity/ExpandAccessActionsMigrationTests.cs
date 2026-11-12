using Identity.Domain.Authorization;
using Identity.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace UnitTests.Identity;

/// <summary>Saved roles keep what «Редагування» let them do when a page gains «Створення» or «Видалення».</summary>
[TestFixture]
public sealed class ExpandAccessActionsMigrationTests
{
    [TestCase(AccessCatalog.Embroidery, 10)]
    [TestCase(AccessCatalog.FabricStock, 10)]
    [TestCase(AccessCatalog.Accounting, 10)]
    [TestCase(AccessCatalog.Payroll, 2)]
    public void A_role_with_edit_gets_the_new_actions(string page, int added)
    {
        var builder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
        new TestableMigration().RunUp(builder);
        var sql = builder.Operations.OfType<SqlOperation>().Select(x => x.Sql).Single(x => x.Contains($"'{{{page}}}'"));

        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain($"(grants->>'{page}')::int | {added})"));
            Assert.That(sql, Does.Contain($"((grants->>'{page}')::int & 4) = 4"));
            Assert.That(AccessCatalog.Find(page)!.Actions.HasFlag((AccessAction)added), Is.True);
        });
    }

    private sealed class TestableMigration : ExpandAccessActions
    {
        public void RunUp(MigrationBuilder builder) => Up(builder);
    }
}
