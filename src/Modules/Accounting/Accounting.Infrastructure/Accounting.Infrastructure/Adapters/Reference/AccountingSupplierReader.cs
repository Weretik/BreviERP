using System.Globalization;

using Accounting.Application.Abstractions;

using Reference.Contracts.Suppliers;

namespace Accounting.Infrastructure.Adapters.Reference;

/// <summary>«Постачальники» (Reference) for the operations of «Бухгалтерія».</summary>
internal sealed class AccountingSupplierReader(ISupplierDirectory suppliers) : IAccountingSupplierReader
{
    public async Task<IReadOnlyList<AccountingParty>> ListAsync(CancellationToken cancellationToken) =>
        (await suppliers.ListAsync(cancellationToken))
        .Select(x => new AccountingParty(x.Id.ToString(CultureInfo.InvariantCulture), x.Name, string.Empty, x.IsActive,
            x.KeepinCrmId))
        .ToArray();
}
