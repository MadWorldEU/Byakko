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
COPY ["src/MadWorldEU.Byakko.Controller.Status/Status.csproj", "src/MadWorldEU.Byakko.Controller.Status/"]
COPY ["src/MadWorldEU.Byakko.Core.BuildingBlocks/BuildingBlocks.csproj", "src/MadWorldEU.Byakko.Core.BuildingBlocks/"]
COPY ["src/MadWorldEU.Byakko.Core.Domain/Domain.csproj", "src/MadWorldEU.Byakko.Core.Domain/"]
COPY ["src/MadWorldEU.Byakko.Infrastructure.ObjectStorage/ObjectStorage.csproj", "src/MadWorldEU.Byakko.Infrastructure.ObjectStorage/"]
COPY ["src/MadWorldEU.Byakko.Infrastructure.Postgresql/Postgresql.csproj", "src/MadWorldEU.Byakko.Infrastructure.Postgresql/"]
RUN dotnet restore "src/MadWorldEU.Byakko.Controller.Status/Status.csproj"
COPY src/ src/
WORKDIR "/src/src/MadWorldEU.Byakko.Controller.Status"
RUN dotnet build "./Status.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Status.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false
RUN rm /app/publish/wwwroot/.well-known/security.txt.br /app/publish/wwwroot/.well-known/security.txt.gz

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Status.dll"]