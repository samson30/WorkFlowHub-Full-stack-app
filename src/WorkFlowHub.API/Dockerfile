FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/WorkFlowHub.API/WorkFlowHub.API.csproj src/WorkFlowHub.API/
COPY src/WorkFlowHub.Core/WorkFlowHub.Core.csproj src/WorkFlowHub.Core/
COPY src/WorkFlowHub.Infrastructure/WorkFlowHub.Infrastructure.csproj src/WorkFlowHub.Infrastructure/
RUN dotnet restore src/WorkFlowHub.API/WorkFlowHub.API.csproj

COPY src/ src/
WORKDIR /src/src/WorkFlowHub.API
RUN dotnet build WorkFlowHub.API.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish WorkFlowHub.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkFlowHub.API.dll"]
