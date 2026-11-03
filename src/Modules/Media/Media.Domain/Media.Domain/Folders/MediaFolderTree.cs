namespace Media.Domain.Folders;

/// <summary>Parent links of all folders. The library holds few folders, so handlers load the whole tree.</summary>
public sealed class MediaFolderTree
{
    public const int MaxDepth = 8;

    private readonly Dictionary<int, int?> _parents;

    public MediaFolderTree(IEnumerable<(int Id, int? ParentId)> folders) =>
        _parents = folders.ToDictionary(x => x.Id, x => x.ParentId);

    public bool Contains(int id) => _parents.ContainsKey(id);

    /// <summary>1 for a top-level folder.</summary>
    public int DepthOf(int id)
    {
        var depth = 0;
        for (int? current = id; current is { } value && _parents.TryGetValue(value, out var parent); current = parent)
        {
            depth++;
            if (depth > _parents.Count) break;
        }

        return depth;
    }

    /// <summary>The folder and every folder below it.</summary>
    public IReadOnlyCollection<int> SelfAndDescendants(int id)
    {
        var result = new HashSet<int> { id };
        var added = true;
        while (added)
        {
            added = false;
            foreach (var (child, parent) in _parents)
            {
                if (parent is { } p && result.Contains(p) && result.Add(child)) added = true;
            }
        }

        return result;
    }

    /// <summary>A new folder fits under the parent when the depth limit allows it.</summary>
    public bool CanAddUnder(int? parentId) => parentId is not { } parent || DepthOf(parent) < MaxDepth;

    /// <summary>A folder cannot move into itself or below itself, and the moved branch must stay within the depth limit.</summary>
    public bool CanMove(int id, int? newParentId)
    {
        if (newParentId is not { } parent) return BranchHeight(id) <= MaxDepth;
        var branch = SelfAndDescendants(id);
        if (branch.Contains(parent)) return false;
        return DepthOf(parent) + BranchHeight(id) <= MaxDepth;
    }

    private int BranchHeight(int id)
    {
        var children = _parents.Where(x => x.Value == id).Select(x => x.Key).ToList();
        return children.Count == 0 ? 1 : 1 + children.Max(BranchHeight);
    }
}
