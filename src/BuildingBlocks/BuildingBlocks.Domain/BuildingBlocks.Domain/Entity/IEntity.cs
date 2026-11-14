
namespace BuildingBlocks.Domain.Entity;

public interface IEntity<out TId>
{
    TId Id { get; }
}
