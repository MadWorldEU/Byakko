FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["src/MadWorldEU.Byakko.Controller.Admin/Admin.csproj", "src/MadWorldEU.Byakko.Controller.Admin/"]
COPY ["src/MadWorldEU.Byakko.Core.Contracts/Contracts.csproj", "src/MadWorldEU.Byakko.Core.Contracts/"]
RUN dotnet restore "src/MadWorldEU.Byakko.Controller.Admin/Admin.csproj"
COPY . .
WORKDIR "/src/src/MadWorldEU.Byakko.Controller.Admin"
RUN dotnet publish "./Admin.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/MadWorldEU.Byakko.Controller.Admin/DockerConfigs/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80