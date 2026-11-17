using Media.Domain.Folders;

namespace UnitTests.Media;

public sealed class MediaFolderTreeTests
{
    // 1 Товари → 2 Куртки → 3 Зимові; 4 Сайт
    private static readonly MediaFolderTree Tree = Of((1, null), (2, 1), (3, 2), (4, null));

    [Test]
    public void Folder_cannot_move_into_itself_or_its_subfolder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Tree.CanMove(new MediaFolderId(1), new MediaFolderId(3)), Is.False);
            Assert.That(Tree.CanMove(new MediaFolderId(1), new MediaFolderId(1)), Is.False);
            Assert.That(Tree.CanMove(new MediaFolderId(2), new MediaFolderId(4)), Is.True);
            Assert.That(Tree.CanMove(new MediaFolderId(3), null), Is.True);
        });
    }

    [Test]
    public void Self_and_descendants_include_the_whole_branch()
    {
        Assert.That(Tree.SelfAndDescendants(new MediaFolderId(1)), Is.EquivalentTo(new[] { new MediaFolderId(1), new MediaFolderId(2), new MediaFolderId(3) }));
    }

    [Test]
    public void Depth_limit_stops_too_deep_folders()
    {
        var chain = Enumerable.Range(1, MediaFolderTree.MaxDepth).Select(id => (id, id == 1 ? (int?)null : id - 1));
        var deep = Of([.. chain]);
        Assert.Multiple(() =>
        {
            Assert.That(deep.DepthOf(new MediaFolderId(MediaFolderTree.MaxDepth)), Is.EqualTo(MediaFolderTree.MaxDepth));
            Assert.That(deep.CanAddUnder(new MediaFolderId(MediaFolderTree.MaxDepth)), Is.False);
            Assert.That(deep.CanAddUnder(new MediaFolderId(MediaFolderTree.MaxDepth - 1)), Is.True);
        });
    }

    private static MediaFolderTree Of(params (int Id, int? ParentId)[] folders) =>
        new(folders.Select(x => (new MediaFolderId(x.Id), x.ParentId is { } parent ? new MediaFolderId(parent) : (MediaFolderId?)null)));
}
