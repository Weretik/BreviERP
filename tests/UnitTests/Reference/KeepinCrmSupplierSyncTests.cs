using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Reference.Domain.Suppliers;
using Reference.Application.Abstractions;
using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

namespace UnitTests.Reference;

public sealed class KeepinCrmSupplierSyncTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    private static KeepinCrmSupplierSnapshot Keepin(string id, string name, string? purchases = null) =>
        new(id, name, purchases, null, null, "0501112233", null, null);

    private static (KeepinCrmSupplierSync Sync, List<Supplier> Added, Mock<IReferenceRepository<Supplier>> Repository)
        Create(IReadOnlyList<KeepinCrmSupplierSnapshot> keepin, List<Supplier> stored)
    {
        var source = new Mock<IKeepinCrmSupplierSource>();
        source.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(keepin);
        var added = new List<Supplier>();
        var repository = new Mock<IReferenceRepository<Supplier>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        repository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Supplier>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Supplier>, CancellationToken>((rows, _) => added.AddRange(rows))
            .ReturnsAsync((IEnumerable<Supplier> rows, CancellationToken _) => rows);
        var sync = new KeepinCrmSupplierSync(
            source.Object, repository.Object, new FakeTimeProvider(Now), NullLogger<KeepinCrmSupplierSync>.Instance);
        return (sync, added, repository);
    }

    [Test]
    public async Task Updates_by_id_links_by_name_creates_the_rest_and_keeps_brevi_fields()
    {
        var linked = Supplier.CreateFromKeepinCrm(SupplierId.From(3), Keepin("220810", "Фастекст"), Now.AddDays(-30)).Value;
        linked.SetActive(false);
        var manual = Supplier.Create(SupplierId.From(6), "Скай Текс, ТОВ", notes: "оптова знижка").Value;
        var gone = Supplier.CreateFromKeepinCrm(SupplierId.From(9), Keepin("1", "Старий"), Now.AddDays(-30)).Value;
        var (sync, added, repository) = Create(
        [
            Keepin("220810", "Фастекст юа", "фурнітура"),
            Keepin("777", "Скай Текс", "тканина"),
            Keepin("104551", "ТК-Домашній текстиль", "тканина"),
            Keepin("104551", "ТК-Домашній текстиль (дубль)")
        ], [linked, manual, gone]);

        var result = await sync.RunAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That((result.Received, result.Updated, result.Linked, result.Created), Is.EqualTo((4, 1, 1, 1)));
            Assert.That(result.MissingInKeepinCrm, Is.EqualTo(new[] { "Старий" }));
            Assert.That(linked.Name, Is.EqualTo("Фастекст юа"));
            Assert.That(linked.PurchaseCategories, Is.EqualTo("фурнітура"));
            Assert.That(linked.IsActive, Is.False);
            Assert.That(manual.KeepinCrmId, Is.EqualTo("777"));
            Assert.That(manual.Name, Is.EqualTo("Скай Текс"));
            Assert.That(manual.Notes, Is.EqualTo("оптова знижка"));
            Assert.That(added.Single().Id.Value, Is.EqualTo(10));
            Assert.That(added.Single().KeepinCrmId, Is.EqualTo("104551"));
            Assert.That(added.Single().IsActive, Is.True);
            Assert.That(gone.KeepinCrmId, Is.EqualTo("1"));
        });
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Name_taken_by_another_supplier_gets_the_keepin_id()
    {
        var first = Supplier.Create(SupplierId.From(1), "Тексика").Value;
        var second = Supplier.Create(SupplierId.From(2), "ТЕКСИКА ТОВ").Value;
        var (sync, added, _) = Create([Keepin("5", "Тексика")], [first, second]);

        await sync.RunAsync(CancellationToken.None);

        Assert.That(added.Single().Name, Is.EqualTo("Тексика (KeepinCRM 5)"));
    }

    [Test]
    public async Task Empty_answer_changes_nothing()
    {
        var (sync, _, repository) = Create([], [Supplier.Create(SupplierId.From(1), "Тексика").Value]);

        await Assert.ThrowsAsync<KeepinCrmUnavailableException>(() => sync.RunAsync(CancellationToken.None));
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
