
namespace Reference.Application.Features.Supplier.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(int Id, UpdateSupplierCommandRequest Request) : ICommand<Result>;
