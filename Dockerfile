FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore MarketPlace.Api/MarketPlace.Api.csproj
RUN dotnet publish MarketPlace.Api/MarketPlace.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migration-build
WORKDIR /src
COPY . .
RUN dotnet tool install --tool-path /tools dotnet-ef --version 10.0.12
RUN ConnectionStrings__Database='Host=localhost;Username=postgres;Password=build;Database=marketplace;Port=5432' \
    /tools/dotnet-ef migrations bundle \
    --project MarketPlace.Api/MarketPlace.Api.csproj \
    --startup-project MarketPlace.Api/MarketPlace.Api.csproj \
    --configuration Release \
    --target-runtime linux-x64 \
    --output /out/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations
WORKDIR /app
COPY --from=migration-build /out/efbundle .
ENTRYPOINT ["/app/efbundle"]

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MarketPlace.Api.dll"]
