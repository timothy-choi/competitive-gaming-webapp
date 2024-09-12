FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SingleGameApi.csproj", "."]
RUN dotnet restore "SingleGameApi.csproj"
COPY . .
RUN dotnet build "SingleGameApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SingleGameApi.csproj" -c Release -o /app/publish

# Create the final image with the runtime and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json .
ENTRYPOINT ["dotnet", "SingleGameApi.dll"]