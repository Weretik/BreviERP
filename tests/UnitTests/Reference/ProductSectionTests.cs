using BuildingBlocks.Domain.Results;
using Moq;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductSection.GetList;
using Reference.Application.Features.ProductSection.Import;
using Reference.Application.Features.ProductSection.Import.DTOs;
using Reference.Application.Features.ProductSection.Import.Validators;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Domain.ProductSections.Entities;
using Reference.Domain.ProductSections.Enums;
using Reference.Domain.ProductSections.ValueObjects;

namespace UnitTests.Reference;

/// <summary>«Типи і розділи»: product sections from the Calculator sheet (REF-005).</summary>
public sealed class ProductSectionTests
{
    private static ProductSection Stored(int id, ProductSectionType type, string name, bool active = true,
        string? comment = null, int order = 0) =>
        ProductSection.Create(ProductSectionId.From(id), new ProductSectionDetails(type, name, active, comment, order)).Value;

    private static DomainResult<ProductSection> Attempt(int id, ProductSectionType type, string name, bool active = true,
        string? comment = null, int order = 0) =>
        ProductSection.Create(ProductSectionId.From(id), new ProductSectionDetails(type, name, active, comment, order));

    private static ProductSectionImportRow Row(int row, string type, string name, bool active = true,
        string? comment = null) => new(row, type, name, active, comment);

    private static (Mock<IReferenceRepository<ProductSection>> Repository, List<ProductSection> Added,
        List<ProductSection> Deleted) Repository(List<ProductSection> stored)
    {
        var added = new List<ProductSection>();
        var deleted = new List<ProductSection>();
        var repository = new Mock<IReferenceRepository<ProductSection>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        repository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProductSection>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ProductSection>, CancellationToken>((rows, _) => added.AddRange(rows))
            .ReturnsAsync((IEnumerable<ProductSection> rows, CancellationToken _) => rows);
        repository.Setup(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<ProductSection>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ProductSection>, CancellationToken>((rows, _) => deleted.AddRange(rows))
            .ReturnsAsync(0);
        return (repository, added, deleted);
    }

    private static async Task<ProductSectionImportResult> Import(Mock<IReferenceRepository<ProductSection>> repository,
        IReadOnlyList<ProductSectionImportRow> rows, bool dryRun = false, bool deleteMissing = false) =>
        (await new ImportProductSectionsCommandHandler(repository.Object)
            .Handle(new ImportProductSectionsCommand(rows, dryRun, deleteMissing), CancellationToken.None)).Value;

    [Test]
    public void Section_needs_a_known_type_and_a_name_and_trims_text()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Attempt(1, (ProductSectionType)7, "Одяг ЛІТО").IsFailure, Is.True);
            Assert.That(Attempt(1, ProductSectionType.Sewing, "  ").IsFailure, Is.True);
            Assert.That(Attempt(1, ProductSectionType.Sewing, new string('я', 201)).IsFailure, Is.True);
            var section = Stored(1, ProductSectionType.Ppe, "  Захист рук ", comment: " ");
            Assert.That(section.Name, Is.EqualTo("Захист рук"));
            Assert.That(section.Comment, Is.Null);
        });
    }

    [Test]
    public async Task Import_creates_updates_and_deletes_by_name_and_takes_row_order()
    {
        var summer = Stored(1, ProductSectionType.Sewing, "Одяг ЛІТО", comment: "Літній одяг", order: 2);
        var old = Stored(2, ProductSectionType.Sewing, "Старий розділ");
        var (repository, added, deleted) = Repository([summer, old]);

        var result = await Import(repository,
        [
            Row(2, "Sewing", "одяг літо", comment: "Літній одяг"),
            Row(3, "Ppe", "Захист рук", active: false),
        ], deleteMissing: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.Applied, Is.True);
            Assert.That((result.Created, result.Updated, result.Unchanged), Is.EqualTo((1, 1, 0)));
            Assert.That(result.Rows[0].Changes, Is.EqualTo(new[] { "Назва" }));
            Assert.That(summer.Name, Is.EqualTo("одяг літо"));
            Assert.That(added.Single().Id.Value, Is.EqualTo(3));
            Assert.That(added.Single().Type, Is.EqualTo(ProductSectionType.Ppe));
            Assert.That(added.Single().IsActive, Is.False);
            Assert.That(added.Single().SortOrder, Is.EqualTo(3));
            Assert.That(deleted, Is.EqualTo(new[] { old }));
            Assert.That(result.Deleted, Is.EqualTo(new[] { "Старий розділ" }));
        });
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Preview_or_a_row_with_an_unknown_type_saves_nothing()
    {
        var (repository, added, _) = Repository([]);

        var preview = await Import(repository, [Row(2, "Sewing", "Одяг ЗИМА")], dryRun: true);
        var broken = await Import(repository, [Row(2, "Sewing", "Одяг ЗИМА"), Row(3, "Shoes", "Взуття")]);

        Assert.Multiple(() =>
        {
            Assert.That(preview.Applied, Is.False);
            Assert.That(preview.Created, Is.EqualTo(1));
            Assert.That(broken.Applied, Is.False);
            Assert.That(broken.Errors, Is.EqualTo(1));
            Assert.That(broken.Rows[1].Action, Is.EqualTo(ReferenceImportActions.Error));
            Assert.That(added, Is.Empty);
        });
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Repeating_the_same_rows_changes_nothing()
    {
        var (repository, _, _) = Repository([Stored(1, ProductSectionType.Ppe, "ЗІЗ", comment: "Засоби", order: 7)]);

        var result = await Import(repository, [Row(7, "Ppe", "ЗІЗ", comment: "Засоби")]);

        Assert.That((result.Created, result.Updated, result.Unchanged), Is.EqualTo((0, 0, 1)));
    }

    [Test]
    public void File_with_repeated_names_is_rejected()
    {
        var validation = new ImportProductSectionsCommandValidator().Validate(new ImportProductSectionsCommand(
            [Row(2, "Sewing", "Одяг ЛІТО"), Row(3, "Sewing", "одяг літо")], true, false));

        Assert.That(validation.IsValid, Is.False);
    }

    [Test]
    public async Task List_shows_sewing_first_then_ppe_in_sheet_order()
    {
        var repository = new Mock<IReferenceReadRepository<ProductSection>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Stored(1, ProductSectionType.Ppe, "ЗІЗ", order: 7),
            Stored(2, ProductSectionType.Sewing, "Одяг ЗИМА", order: 5),
            Stored(3, ProductSectionType.Sewing, "Одяг ЛІТО", order: 2),
        ]);

        var rows = (await new GetProductSectionsQueryHandler(repository.Object)
            .Handle(new GetProductSectionsQuery(), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(rows.Select(x => x.Name), Is.EqualTo(new[] { "Одяг ЛІТО", "Одяг ЗИМА", "ЗІЗ" }));
            Assert.That(rows[2].Type, Is.EqualTo("Ppe"));
        });
    }
}
