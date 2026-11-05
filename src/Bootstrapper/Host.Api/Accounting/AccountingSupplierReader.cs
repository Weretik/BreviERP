using System.Globalization;
using Accounting.Application.Abstractions;
using Mediator;
using Reference.Application.Features.Supplier.GetList;

namespace Host.Api.Accounting;

/// <summary>«Постачальники» (Reference) for the operations of «Бухгалтерія».</summary>
internal sealed class AccountingSupplierReader(ISender sender) : IAccountingSupplierReader
{
    public async Task<IReadOnlyList<AccountingParty>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference suppliers could not be read.");
        return result.Value
            .Select(x => new AccountingParty(x.Id.ToString(CultureInfo.InvariantCulture), x.Name, string.Empty, x.IsActive))
            .ToArray();
    }
}
