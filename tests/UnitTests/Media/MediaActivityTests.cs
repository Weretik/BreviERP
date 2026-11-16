using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Media.Application.Features.Activity.EventHandlers;
using Media.Domain.Files;
using Media.Domain.Folders;

namespace UnitTests.Media;

/// <summary>Photos and folders read as «Журнал дій» entries (ACT-001).</summary>
public sealed class MediaActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    private static MediaFile Upload(string? sourceKey = null) => MediaFile.Create("fartukh.jpg", null, "image/jpeg", 1024,
        "cdn", "/brevi/a.jpg", "https://cdn/a.jpg", Now, sourceKey).Value;

    [Test]
    public void An_uploaded_photo_is_listed_but_an_imported_one_is_not()
    {
        var entry = new MediaFileActivity().Describe(((IHasDomainEvents)Upload()).DomainEvents.Single());

        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.SubjectTitle, Is.EqualTo("fartukh.jpg"));
        Assert.That(((IHasDomainEvents)Upload("old-site/1.jpg")).DomainEvents, Is.Empty);
    }

    [Test]
    public void Renaming_a_photo_lists_the_name_and_saving_it_unchanged_lists_nothing()
    {
        var unchanged = Upload();
        ((IHasDomainEvents)unchanged).ClearDomainEvents();
        unchanged.Update("fartukh.jpg", null, null, Now);
        var renamed = Upload();
        ((IHasDomainEvents)renamed).ClearDomainEvents();
        renamed.Update("fartukh-blue.jpg", null, null, Now);

        var describer = new MediaFileActivity();
        Assert.That(describer.Describe(((IHasDomainEvents)unchanged).DomainEvents.Single()), Is.Null);
        var change = describer.Describe(((IHasDomainEvents)renamed).DomainEvents.Single())!.Changes.Single();
        Assert.That((change.Field, change.From, change.To), Is.EqualTo(("Назва", "fartukh.jpg", "fartukh-blue.jpg")));
    }

    [Test]
    public void A_folder_made_for_an_upload_path_is_not_listed()
    {
        var path = MediaFolder.CreateForPath("products", null, Now).Value;
        var made = MediaFolder.Create("Фартухи", null, Now).Value;

        Assert.That(((IHasDomainEvents)path).DomainEvents, Is.Empty);
        Assert.That(new MediaFolderActivity().Describe(((IHasDomainEvents)made).DomainEvents.Single())!.Action,
            Is.EqualTo(ActivityActions.Created));
    }
}
