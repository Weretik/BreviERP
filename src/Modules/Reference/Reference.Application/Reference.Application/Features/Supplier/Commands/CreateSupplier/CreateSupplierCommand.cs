
namespace Reference.Application.Features.Supplier.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(CreateSupplierCommandRequest Request) : ICommand<Result<int>>;
