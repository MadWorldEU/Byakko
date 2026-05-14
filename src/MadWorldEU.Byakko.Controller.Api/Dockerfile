FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["src/MadWorldEU.Byakko.Controller.Api/Api.csproj", "src/MadWorldEU.Byakko.Controller.Api/"]
COPY ["src/MadWorldEU.Byakko.Core.Application/Application.csproj", "src/MadWorldEU.Byakko.Core.Application/"]
COPY ["src/MadWorldEU.Byakko.Core.Domain/Domain.csproj", "src/MadWorldEU.Byakko.Core.Domain/"]
COPY ["src/MadWorldEU.Byakko.Core.BuildingBlocks/BuildingBlocks.csproj", "src/MadWorldEU.Byakko.Core.BuildingBlocks/"]
COPY ["src/MadWorldEU.Byakko.Core.Contracts/Contracts.csproj", "src/MadWorldEU.Byakko.Core.Contracts/"]
COPY ["src/MadWorldEU.Byakko.Infrastructure.Postgresql/Postgresql.csproj", "src/MadWorldEU.Byakko.Infrastructure.Postgresql/"]
COPY ["src/MadWorldEU.Byakko.Infrastructure.ObjectStorage/ObjectStorage.csproj", "src/MadWorldEU.Byakko.Infrastructure.ObjectStorage/"]
RUN dotnet restore "src/MadWorldEU.Byakko.Controller.Api/Api.csproj"
COPY . .
WORKDIR "/src/src/MadWorldEU.Byakko.Controller.Api"
RUN dotnet build "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
