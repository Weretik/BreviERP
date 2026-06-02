namespace Reference.Application.Features.Supplier.Delete;

public sealed record DeleteSupplierCommand(int Id) : ICommand<Result>;
