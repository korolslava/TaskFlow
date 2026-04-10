FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:${PORT}

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TaskFlow.API/TaskFlow.API.csproj", "src/TaskFlow.API/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]
COPY ["src/TaskFlow.Application/TaskFlow.Application.csproj", "src/TaskFlow.Application/"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]
RUN dotnet restore "src/TaskFlow.API/TaskFlow.API.csproj"
COPY . .
RUN dotnet build "src/TaskFlow.API/TaskFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TaskFlow.API/TaskFlow.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]