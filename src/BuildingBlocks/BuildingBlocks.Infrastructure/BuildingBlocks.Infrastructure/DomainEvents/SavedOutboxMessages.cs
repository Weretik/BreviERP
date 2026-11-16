using BuildingBlocks.Application.Outbox;

namespace BuildingBlocks.Infrastructure.DomainEvents;

public sealed class SavedOutboxMessages : ISavedOutboxMessages
{
    private readonly List<Guid> _ids = [];

    public void Add(IEnumerable<Guid> ids) => _ids.AddRange(ids);

    public IReadOnlyCollection<Guid> Take()
    {
        var ids = _ids.ToArray();
        _ids.Clear();
        return ids;
    }
}
