using Reference.Application.Features.Supplier.Update.DTOs;

namespace Reference.Application.Features.Supplier.Update;

public sealed record UpdateSupplierCommand(int Id, UpdateSupplierCommandRequest Request) : ICommand<Result>;
