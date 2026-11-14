using Accounting.Api.Endpoints.Accounts;
using Accounting.Api.Endpoints.Attachments;
using Accounting.Api.Endpoints.Categories;
using Accounting.Api.Endpoints.Counterparties;
using Accounting.Api.Endpoints.Import;
using Accounting.Api.Endpoints.Overview;
using Accounting.Api.Endpoints.Reports;
using Accounting.Api.Endpoints.Settings;
using Accounting.Api.Endpoints.Statements;
using Accounting.Api.Endpoints.Transactions;
using Accounting.Api.Endpoints.Transfers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using BuildingBlocks.Application.DependencyInjection;
using Accounting.Application;
using Accounting.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounting.Api.DependencyInjection;

public static class AccountingModuleExtensions
{
    /// <summary>Registers the Accounting module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountingInfrastructure(configuration);
        services.AddModuleValidators(typeof(AccountingApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Accounting module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder app) =>
        app.MapFinanceAccountsEndpoints().MapFinanceAttachmentsEndpoints().MapFinanceCategoriesEndpoints()
            .MapFinanceCounterpartiesEndpoints().MapLegacyFinanceImportEndpoints().MapFinanceOverviewEndpoints()
            .MapFinanceAuditEndpoints().MapFinanceExportEndpoints().MapFinanceReportsEndpoints()
            .MapAccountingSettingsEndpoints().MapBankStatementsEndpoints().MapFinanceTransactionsEndpoints()
            .MapFinanceTransfersEndpoints();
}
