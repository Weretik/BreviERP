FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Bootstrapper/Host.Seed/Host.Seed.csproj", "src/Bootstrapper/Host.Seed/"]
RUN dotnet restore "src/Bootstrapper/Host.Seed/Host.Seed.csproj"
COPY . .
WORKDIR "/src/src/Bootstrapper/Host.Seed"
RUN dotnet build "./Host.Seed.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Host.Seed.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Host.Seed.dll"]
