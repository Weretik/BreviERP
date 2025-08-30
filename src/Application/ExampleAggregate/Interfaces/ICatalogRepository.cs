namespace Application.ExampleAggregate.Interfaces;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
