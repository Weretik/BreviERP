FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["BreviERP.sln", "./"]
COPY ["src/Bootstrapper/Host.Api/Host.Api.csproj", "src/Bootstrapper/Host.Api/"]

COPY ["src/BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "src/BuildingBlocks/BuildingBlocks.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/"]

COPY ["src/Modules/Accounting/Accounting.Api/Accounting.Api/Accounting.Api.csproj", "src/Modules/Accounting/Accounting.Api/Accounting.Api/"]
COPY ["src/Modules/Accounting/Accounting.Application/Accounting.Application/Accounting.Application.csproj", "src/Modules/Accounting/Accounting.Application/Accounting.Application/"]
COPY ["src/Modules/Accounting/Accounting.Domain/Accounting.Domain/Accounting.Domain.csproj", "src/Modules/Accounting/Accounting.Domain/Accounting.Domain/"]
COPY ["src/Modules/Accounting/Accounting.Infrastructure/Accounting.Infrastructure/Accounting.Infrastructure.csproj", "src/Modules/Accounting/Accounting.Infrastructure/Accounting.Infrastructure/"]

COPY ["src/Modules/Crm/Crm.Api/Crm.Api/Crm.Api.csproj", "src/Modules/Crm/Crm.Api/Crm.Api/"]
COPY ["src/Modules/Crm/Crm.Application/Crm.Application/Crm.Application.csproj", "src/Modules/Crm/Crm.Application/Crm.Application/"]
COPY ["src/Modules/Crm/Crm.Domain/Crm.Domain/Crm.Domain.csproj", "src/Modules/Crm/Crm.Domain/Crm.Domain/"]
COPY ["src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj", "src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/"]

COPY ["src/Modules/Identity/Identity.Api/Identity.Api/Identity.Api.csproj", "src/Modules/Identity/Identity.Api/Identity.Api/"]
COPY ["src/Modules/Identity/Identity.Application/Identity.Application/Identity.Application.csproj", "src/Modules/Identity/Identity.Application/Identity.Application/"]
COPY ["src/Modules/Identity/Identity.Domain/Identity.Domain/Identity.Domain.csproj", "src/Modules/Identity/Identity.Domain/Identity.Domain/"]
COPY ["src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/"]

COPY ["src/Modules/Reference/Reference.Api/Reference.Api/Reference.Api.csproj", "src/Modules/Reference/Reference.Api/Reference.Api/"]
COPY ["src/Modules/Reference/Reference.Application/Reference.Application/Reference.Application.csproj", "src/Modules/Reference/Reference.Application/Reference.Application/"]
COPY ["src/Modules/Reference/Reference.Domain/Reference.Domain/Reference.Domain.csproj", "src/Modules/Reference/Reference.Domain/Reference.Domain/"]
COPY ["src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Reference.Infrastructure.csproj", "src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/"]

RUN dotnet restore "src/Bootstrapper/Host.Api/Host.Api.csproj"
COPY . .
RUN dotnet publish "src/Bootstrapper/Host.Api/Host.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Host.Api.dll"]
