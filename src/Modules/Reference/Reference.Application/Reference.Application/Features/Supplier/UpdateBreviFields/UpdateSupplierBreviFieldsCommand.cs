namespace Reference.Application.Features.Supplier.UpdateBreviFields;

/// <summary>Changes the fields Brevi always owns, for any supplier including a KeepinCRM one.</summary>
public sealed record UpdateSupplierBreviFieldsCommand(int Id, bool IsActive, string? Notes) : ICommand<Result>;
