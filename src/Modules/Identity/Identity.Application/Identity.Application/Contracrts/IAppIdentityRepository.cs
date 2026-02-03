namespace Identity.Application.Contracrts;

public interface IAppIdentityRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
