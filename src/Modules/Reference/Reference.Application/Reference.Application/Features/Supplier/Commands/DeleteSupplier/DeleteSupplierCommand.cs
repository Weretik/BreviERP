namespace Reference.Application.Features.Supplier.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand(int Id) : ICommand<Result>;
