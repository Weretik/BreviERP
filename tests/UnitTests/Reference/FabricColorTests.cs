using BuildingBlocks.Domain.Results;
using Ardalis.Result;
using Moq;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.FabricColor.Create;
using Reference.Application.Features.FabricColor.GetList;
using Reference.Application.Features.FabricColor.Shared;
using Reference.Application.Features.FabricColor.Update;
using Reference.Domain.FabricColors.Entities;
using Reference.Domain.FabricColors.ValueObjects;

namespace UnitTests.Reference;

/// <summary>«Кольори» of «Матеріали» (REF-006).</summary>
public sealed class FabricColorTests
{
    private static FabricColor Stored(int id, string name, bool active = true, int order = 0) =>
        FabricColor.Create(FabricColorId.From(id), new FabricColorDetails(name, active, order)).Value;

    private static DomainResult<FabricColor> Attempt(int id, string name, bool active = true, int order = 0) =>
        FabricColor.Create(FabricColorId.From(id), new FabricColorDetails(name, active, order));

    private static (Mock<IReferenceRepository<FabricColor>> Repository, List<FabricColor> Added) Repository(
        List<FabricColor> stored)
    {
        var added = new List<FabricColor>();
        var repository = new Mock<IReferenceRepository<FabricColor>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        repository.Setup(x => x.AddAsync(It.IsAny<FabricColor>(), It.IsAny<CancellationToken>()))
            .Callback<FabricColor, CancellationToken>((row, _) => added.Add(row))
            .ReturnsAsync((FabricColor row, CancellationToken _) => row);
        return (repository, added);
    }

    [Test]
    public void Colour_needs_a_name_of_at_most_80_characters_and_trims_it()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Attempt(1, "  ").IsFailure, Is.True);
            Assert.That(Attempt(1, new string('я', 81)).IsFailure, Is.True);
            Assert.That(Stored(1, "  Темно-синій кол.1076 ").Name, Is.EqualTo("Темно-синій кол.1076"));
        });
    }

    [Test]
    public async Task Create_takes_the_next_id_and_rejects_the_same_colour_with_other_case_or_spaces()
    {
        var (repository, added) = Repository([Stored(1, "Чорний"), Stored(4, "Світло-сірий")]);
        var handler = new CreateFabricColorCommandHandler(repository.Object);

        var created = await handler.Handle(new(new FabricColorFields("Вафля", SortOrder: 61)), CancellationToken.None);
        var duplicate = await handler.Handle(new(new FabricColorFields("світло - сірий")), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(created.Value, Is.EqualTo(5));
            Assert.That(added.Single().Name, Is.EqualTo("Вафля"));
            Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(duplicate.ValidationErrors.Single().ErrorMessage, Is.EqualTo(FabricColorNames.Duplicate));
        });
    }

    [Test]
    public async Task Update_renames_and_hides_but_not_onto_another_colour()
    {
        var black = Stored(1, "Чорний");
        var (repository, _) = Repository([black, Stored(2, "Синій")]);
        var handler = new UpdateFabricColorCommandHandler(repository.Object);

        var clash = await handler.Handle(new(1, new FabricColorFields("СИНІЙ")), CancellationToken.None);
        var saved = await handler.Handle(new(1, new FabricColorFields("Чорний ", false, 3)), CancellationToken.None);
        var missing = await handler.Handle(new(9, new FabricColorFields("Хакі")), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(clash.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(saved.IsSuccess, Is.True);
            Assert.That((black.IsActive, black.SortOrder), Is.EqualTo((false, 3)));
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task List_is_in_list_order()
    {
        var repository = new Mock<IReferenceReadRepository<FabricColor>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([Stored(3, "Синій", order: 2), Stored(1, "Чорний", order: 1), Stored(2, "Вафля", false, 61)]);

        var rows = (await new GetFabricColorsQueryHandler(repository.Object)
            .Handle(new GetFabricColorsQuery(), CancellationToken.None)).Value;

        Assert.That(rows.Select(x => x.Name), Is.EqualTo(new[] { "Чорний", "Синій", "Вафля" }));
    }
}
