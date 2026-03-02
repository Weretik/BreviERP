using Accounting.Domain.Errors;
using Accounting.Domain.ValueObjects;

namespace Accounting.Domain.Entities;

public sealed class TransactionCategory : BaseEntity<TransactionCategoryId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public TransactionCategoryId? ParentId { get; private set; }
    public CategoryPath Path { get; private set; }
    #endregion

    #region Constructors
    private TransactionCategory() { }

    private TransactionCategory(
        TransactionCategoryId id,
        string name,
        CategoryPath path,
        TransactionCategoryId? parentId = null)
    {
        SetCategoryId(id);
        SetName(name);
        SetPath(path);
        SetParentId(parentId);
    }
    #endregion

    #region Factories
    public static TransactionCategory Create(
        TransactionCategoryId id,
        string name,
        CategoryPath path,
        TransactionCategoryId? parentId = null)
        => new(id, name, path, parentId);

    public static TransactionCategory CreateRoot(TransactionCategoryId id, string name)
        => new(
            id,
            name,
            CategoryPath.Root(id.ToString()),
            parentId: null
        );

    public static TransactionCategory CreateChild(
        TransactionCategoryId id,
        string name,
        TransactionCategory parent)
        => new(
            id,
            name,
            parent.Path.Append(id.ToString()),
            parentId: parent.Id
        );

    public void Update(string name, CategoryPath path, TransactionCategoryId? parentId = null)
    {
        SetName(name);
        SetPath(path);
        SetParentId(parentId);
    }
    #endregion

    #region Setters/Validation
    private void SetCategoryId(TransactionCategoryId id)
    {
        if (id.Value <= 0)
            throw new DomainException(TransactionCategoryErrors.IdMustBePositive());
        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(TransactionCategoryErrors.NameIsRequired());

        if (name.Length is < 1 or > 100)
            throw new DomainException(TransactionCategoryErrors.NameLengthInvalid(name.Length));

        Name = name.Trim();
    }

    private void SetPath(CategoryPath path)
    {
        if (path.Value.Length == 0)
            throw new DomainException(TransactionCategoryErrors.PathIsRequired());

        Path = path;
    }

    private void SetParentId(TransactionCategoryId? parentId)
    {
        ParentId = parentId;
    }
    #endregion

    #region TransactionCategory API
    public void Rename(string name) => SetName(name);
    public void Repath(CategoryPath newPath) => SetPath(newPath);
    #endregion

    #region Internal rules (опционально, как в примере)
    protected void GuardReparentInvariant(CategoryPath newParentPath)
    {
        if (newParentPath.Value == Path.Value)
            throw new DomainException(TransactionCategoryErrors.CannotSetSelfAsParent(Path.Value));

        if (IsDescendantOf(newParentPath, Path))
            throw new DomainException(TransactionCategoryErrors.CannotMoveUnderDescendant(
                Path.Value, newParentPath.Value
            ));
    }

    public bool IsRoot => SegmentCount(Path) <= 1;

    private static bool IsDescendantOf(CategoryPath candidateChild, CategoryPath candidateAncestor)
    {
        var anc = candidateAncestor.Value;
        var child = candidateChild.Value;

        return child.Length > anc.Length &&
               child.StartsWith(anc, StringComparison.Ordinal) &&
               child[anc.Length] == '.';
    }

    private static int SegmentCount(CategoryPath path)
        => 1 + path.Value.Count(ch => ch == '.');
    #endregion
}
