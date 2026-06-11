FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TaskFlow.Worker/TaskFlow.Worker.csproj", "src/TaskFlow.Worker/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]

RUN dotnet restore "src/TaskFlow.Worker/TaskFlow.Worker.csproj"

COPY . .

RUN dotnet build "src/TaskFlow.Worker/TaskFlow.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TaskFlow.Worker/TaskFlow.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Worker.dll"]