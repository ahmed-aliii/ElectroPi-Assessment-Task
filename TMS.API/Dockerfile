FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first so Docker can cache package restore between source changes.
COPY ["TMS.API/TMS.API.csproj", "TMS.API/"]
COPY ["TMS.Application/TMS.Application.csproj", "TMS.Application/"]
COPY ["TMS.Domain/TMS.Domain.csproj", "TMS.Domain/"]
COPY ["TMS.Infrastruture/TMS.Infrastructure.csproj", "TMS.Infrastruture/"]
RUN dotnet restore "TMS.API/TMS.API.csproj"

COPY . .
WORKDIR "/src/TMS.API"
RUN dotnet build "TMS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TMS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TMS.API.dll"]