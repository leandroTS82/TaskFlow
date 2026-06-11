FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TaskFlow.Processor/TaskFlow.Processor.csproj", "src/TaskFlow.Processor/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]

RUN dotnet restore "src/TaskFlow.Processor/TaskFlow.Processor.csproj"

COPY . .

RUN dotnet build "src/TaskFlow.Processor/TaskFlow.Processor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TaskFlow.Processor/TaskFlow.Processor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Processor.dll"]