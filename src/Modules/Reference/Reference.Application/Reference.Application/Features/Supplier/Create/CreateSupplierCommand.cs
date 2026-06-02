using Reference.Application.Features.Supplier.Create.DTOs;

namespace Reference.Application.Features.Supplier.Create;

public sealed record CreateSupplierCommand(CreateSupplierCommandRequest Request) : ICommand<Result<int>>;
