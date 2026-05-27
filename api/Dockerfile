# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Glorp.slnx ./
COPY src/Glorp.Api/Glorp.Api.csproj src/Glorp.Api/
RUN dotnet restore src/Glorp.Api/Glorp.Api.csproj

COPY src/ src/
RUN dotnet publish src/Glorp.Api/Glorp.Api.csproj \
    -c Release \
    -o /app/publish \
    -p:GenerateTypeScript=false \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Glorp.Api.dll"]
