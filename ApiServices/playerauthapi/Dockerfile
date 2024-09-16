FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8087

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["playerauthapi.csproj", "."]
RUN dotnet restore "playerauthapi.csproj"
COPY . .
RUN dotnet build "playerauthapi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "playerauthapi.csproj" -c Release -o /app/publish

# Create the final image with the runtime and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json .
ENTRYPOINT ["dotnet", "playerauthapi.dll"]