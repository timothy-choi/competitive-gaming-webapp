FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PlayerApi.csproj", "."]
RUN dotnet restore "PlayerApi.csproj"
COPY . .
RUN dotnet build "PlayerApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PlayerApi.csproj" -c Release -o /app/publish

# Create the final image with the runtime and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlayerApi.dll"]