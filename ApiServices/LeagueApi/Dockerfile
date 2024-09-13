FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LeagueApi.csproj", "."]
RUN dotnet restore "LeagueApi.csproj"
COPY . .
RUN dotnet build "LeagueApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LeagueApi.csproj" -c Release -o /app/publish

# Create the final image with the runtime and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json .
ENTRYPOINT ["dotnet", "LeagueApi.dll"]