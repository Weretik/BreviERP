using System.Collections.Concurrent;
using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>
/// Signs in once to the dealer cabinet and reads the dealer price of each product link with that session, two
/// pages at a time. Missing credentials or a refused sign-in fail the whole read, so no price changes.
/// </summary>
internal sealed class SupplierCabinetPriceReader(SupplierCabinetClient client, IOptions<SupplierCabinetOptions> options)
{
    private const int ParallelPages = 2;

    public async Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        ISupplierCabinet cabinet, IReadOnlyList<PpePriceImportProduct> products, CancellationToken cancellationToken)
    {
        var login = cabinet.Credentials(options.Value);
        if (!login.IsSet)
            return Result.Invalid(new ValidationError("source",
                $"Дані входу в кабінет {cabinet.Name} ще не додано в Google Cloud. Ціни не змінено."));

        string? session;
        try
        {
            session = await cabinet.SignInAsync(client, login, cancellationToken);
        }
        catch (Exception error) when (error is HttpRequestException
                                      || (error is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            return Result.Invalid(new ValidationError("source",
                $"Кабінет {cabinet.Name} не відповів. Ціни не змінено, спробуйте пізніше."));
        }

        if (session is null)
            return Result.Invalid(new ValidationError("source",
                $"Не вдалося увійти в кабінет {cabinet.Name}. Перевірте логін і пароль. Ціни не змінено."));

        var prices = new ConcurrentBag<SupplierFilePrice>();
        await Parallel.ForEachAsync(SupplierLinks.Distinct(products),
            new ParallelOptions { MaxDegreeOfParallelism = ParallelPages, CancellationToken = cancellationToken },
            async (url, token) => prices.Add(await ReadPageAsync(cabinet, session, url, token)));
        return Result.Success<IReadOnlyList<SupplierFilePrice>>(prices.ToList());
    }

    private async Task<SupplierFilePrice> ReadPageAsync(ISupplierCabinet cabinet, string session, string url,
        CancellationToken cancellationToken)
    {
        SupplierFilePrice Problem(string message) => new(null, url, null, 0m, null, null, message);

        if (!SupplierLinks.IsOnSite(url, cabinet.Host))
            return Problem($"Посилання постачальника веде не на сайт {cabinet.Name} ({cabinet.Host}).");
        var page = await client.GetPageAsync(url, session, cancellationToken);
        if (page.Text is null) return Problem(page.Problem!);
        return cabinet.FindPrice(page.Text) is { } price
            ? new SupplierFilePrice(null, url, null, price, null, null, null)
            : Problem("На сторінці кабінету не знайдено дилерську ціну.");
    }
}
