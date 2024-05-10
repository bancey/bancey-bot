FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Bancey.Bot.WorkerService/. .
RUN dotnet build "Bancey.Bot.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bancey.Bot.WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bancey.Bot.WorkerService.dll"]