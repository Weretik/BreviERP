using Reference.Application.Features.Supplier.GetList.DTOs;

namespace Reference.Application.Features.Supplier.GetList;

public sealed record GetSuppliersQuery : IQuery<Result<List<SupplierRowDTO>>>;
