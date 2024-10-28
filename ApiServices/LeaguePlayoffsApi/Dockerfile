FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8088

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LeaguePlayoffsApi.csproj", "."]
RUN dotnet restore "LeaguePlayoffsApi.csproj"
COPY . .
RUN dotnet build "LeaguePlayoffsApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LeaguePlayoffsApi.csproj" -c Release -o /app/publish

# Create the final image with the runtime and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json .
ENTRYPOINT ["dotnet", "LeaguePlayoffsApi.dll"]