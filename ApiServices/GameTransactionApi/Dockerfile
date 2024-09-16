FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8086

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GameTransactionApi.csproj", "."]
RUN dotnet restore "GameTransactionApi.csproj"
COPY . .
RUN dotnet build "GameTransactionApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GameTransactionApi.csproj" -c Release -o /app/publish

# Create the final image with the runtime and the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json .
ENTRYPOINT ["dotnet", "GameTransactionApi.dll"]