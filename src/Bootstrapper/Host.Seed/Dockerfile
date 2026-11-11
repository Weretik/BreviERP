FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["BreviERP.sln", "./"]
COPY ["src/Bootstrapper/Host.Seed/Host.Seed.csproj", "src/Bootstrapper/Host.Seed/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/"]
COPY ["src/Modules/Catalog/Catalog.Application/Catalog.Application/Catalog.Application.csproj", "src/Modules/Catalog/Catalog.Application/Catalog.Application/"]
COPY ["src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Catalog.Domain.csproj", "src/Modules/Catalog/Catalog.Domain/Catalog.Domain/"]
COPY ["src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/"]
COPY ["src/Modules/Accounting/Accounting.Application/Accounting.Application/Accounting.Application.csproj", "src/Modules/Accounting/Accounting.Application/Accounting.Application/"]
COPY ["src/Modules/Accounting/Accounting.Domain/Accounting.Domain/Accounting.Domain.csproj", "src/Modules/Accounting/Accounting.Domain/Accounting.Domain/"]
COPY ["src/Modules/Accounting/Accounting.Infrastructure/Accounting.Infrastructure/Accounting.Infrastructure.csproj", "src/Modules/Accounting/Accounting.Infrastructure/Accounting.Infrastructure/"]
COPY ["src/Modules/Crm/Crm.Application/Crm.Application/Crm.Application.csproj", "src/Modules/Crm/Crm.Application/Crm.Application/"]
COPY ["src/Modules/Crm/Crm.Domain/Crm.Domain/Crm.Domain.csproj", "src/Modules/Crm/Crm.Domain/Crm.Domain/"]
COPY ["src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj", "src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/"]
COPY ["src/Modules/Identity/Identity.Application/Identity.Application/Identity.Application.csproj", "src/Modules/Identity/Identity.Application/Identity.Application/"]
COPY ["src/Modules/Identity/Identity.Domain/Identity.Domain/Identity.Domain.csproj", "src/Modules/Identity/Identity.Domain/Identity.Domain/"]
COPY ["src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/"]
COPY ["src/Modules/Reference/Reference.Application/Reference.Application/Reference.Application.csproj", "src/Modules/Reference/Reference.Application/Reference.Application/"]
COPY ["src/Modules/Reference/Reference.Domain/Reference.Domain/Reference.Domain.csproj", "src/Modules/Reference/Reference.Domain/Reference.Domain/"]
COPY ["src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Reference.Infrastructure.csproj", "src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/"]

COPY ["src/Jobs/CatalogImport/CatalogImport.csproj", "src/Jobs/CatalogImport/"]
COPY ["src/Jobs/SewingPricing/SewingPricing.csproj", "src/Jobs/SewingPricing/"]

COPY ["src/Modules/Reminders/Reminders.Application/Reminders.Application/Reminders.Application.csproj", "src/Modules/Reminders/Reminders.Application/Reminders.Application/"]
COPY ["src/Modules/Reminders/Reminders.Domain/Reminders.Domain/Reminders.Domain.csproj", "src/Modules/Reminders/Reminders.Domain/Reminders.Domain/"]
COPY ["src/Modules/Reminders/Reminders.Infrastructure/Reminders.Infrastructure/Reminders.Infrastructure.csproj", "src/Modules/Reminders/Reminders.Infrastructure/Reminders.Infrastructure/"]
COPY ["src/Modules/Media/Media.Application/Media.Application/Media.Application.csproj", "src/Modules/Media/Media.Application/Media.Application/"]
COPY ["src/Modules/Media/Media.Domain/Media.Domain/Media.Domain.csproj", "src/Modules/Media/Media.Domain/Media.Domain/"]
COPY ["src/Modules/Media/Media.Infrastructure/Media.Infrastructure/Media.Infrastructure.csproj", "src/Modules/Media/Media.Infrastructure/Media.Infrastructure/"]
COPY ["src/Modules/Production/Production.Application/Production.Application/Production.Application.csproj", "src/Modules/Production/Production.Application/Production.Application/"]
COPY ["src/Modules/Production/Production.Domain/Production.Domain/Production.Domain.csproj", "src/Modules/Production/Production.Domain/Production.Domain/"]
COPY ["src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Production.Infrastructure.csproj", "src/Modules/Production/Production.Infrastructure/Production.Infrastructure/"]
COPY ["src/Modules/Purchasing/Purchasing.Application/Purchasing.Application/Purchasing.Application.csproj", "src/Modules/Purchasing/Purchasing.Application/Purchasing.Application/"]
COPY ["src/Modules/Purchasing/Purchasing.Domain/Purchasing.Domain/Purchasing.Domain.csproj", "src/Modules/Purchasing/Purchasing.Domain/Purchasing.Domain/"]
COPY ["src/Modules/Purchasing/Purchasing.Infrastructure/Purchasing.Infrastructure/Purchasing.Infrastructure.csproj", "src/Modules/Purchasing/Purchasing.Infrastructure/Purchasing.Infrastructure/"]
COPY ["src/Modules/Content/Content.Application/Content.Application/Content.Application.csproj", "src/Modules/Content/Content.Application/Content.Application/"]
COPY ["src/Modules/Content/Content.Domain/Content.Domain/Content.Domain.csproj", "src/Modules/Content/Content.Domain/Content.Domain/"]
COPY ["src/Modules/Content/Content.Infrastructure/Content.Infrastructure/Content.Infrastructure.csproj", "src/Modules/Content/Content.Infrastructure/Content.Infrastructure/"]
COPY ["src/Modules/Warehouse/Warehouse.Application/Warehouse.Application/Warehouse.Application.csproj", "src/Modules/Warehouse/Warehouse.Application/Warehouse.Application/"]
COPY ["src/Modules/Warehouse/Warehouse.Domain/Warehouse.Domain/Warehouse.Domain.csproj", "src/Modules/Warehouse/Warehouse.Domain/Warehouse.Domain/"]
COPY ["src/Modules/Warehouse/Warehouse.Infrastructure/Warehouse.Infrastructure/Warehouse.Infrastructure.csproj", "src/Modules/Warehouse/Warehouse.Infrastructure/Warehouse.Infrastructure/"]
COPY ["src/Modules/Ai/Ai.Application/Ai.Application/Ai.Application.csproj", "src/Modules/Ai/Ai.Application/Ai.Application/"]
COPY ["src/Modules/Ai/Ai.Domain/Ai.Domain/Ai.Domain.csproj", "src/Modules/Ai/Ai.Domain/Ai.Domain/"]
COPY ["src/Modules/Ai/Ai.Infrastructure/Ai.Infrastructure/Ai.Infrastructure.csproj", "src/Modules/Ai/Ai.Infrastructure/Ai.Infrastructure/"]

RUN dotnet restore "src/Bootstrapper/Host.Seed/Host.Seed.csproj"
COPY . .
RUN dotnet publish "src/Bootstrapper/Host.Seed/Host.Seed.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Host.Seed.dll"]
