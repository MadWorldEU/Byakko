FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["src/MadWorldEU.Byakko.Controller.Portal/Portal.csproj", "src/MadWorldEU.Byakko.Controller.Portal/"]
COPY ["src/MadWorldEU.Byakko.Core.Contracts/Contracts.csproj", "src/MadWorldEU.Byakko.Core.Contracts/"]
RUN dotnet restore "src/MadWorldEU.Byakko.Controller.Portal/Portal.csproj"
COPY src/ src/
WORKDIR "/src/src/MadWorldEU.Byakko.Controller.Portal"
RUN dotnet publish "./Portal.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/MadWorldEU.Byakko.Controller.Portal/DockerConfigs/nginx.conf /etc/nginx/conf.d/default.conf
RUN rm /usr/share/nginx/html/appsetings.json.br /usr/share/nginx/html/appsetings.json.gz
RUN chown -R nginx:nginx /var/cache/nginx /var/log/nginx /etc/nginx/conf.d /usr/share/nginx/html \
    && touch /var/run/nginx.pid \
    && chown nginx:nginx /var/run/nginx.pid
USER nginx
EXPOSE 8080