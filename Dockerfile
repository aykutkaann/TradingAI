# syntax=docker/dockerfile:1
# ---- Build stage --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first so `dotnet restore` is cached when only source changes.
COPY src/TradingAI.Domain/TradingAI.Domain.csproj         src/TradingAI.Domain/
COPY src/TradingAI.Application/TradingAI.Application.csproj src/TradingAI.Application/
COPY src/TradingAI.Infrastructure/TradingAI.Infrastructure.csproj src/TradingAI.Infrastructure/
COPY src/TradingAI.API/TradingAI.API.csproj               src/TradingAI.API/

RUN dotnet restore src/TradingAI.API/TradingAI.API.csproj

# Now bring in the rest of the source and publish a release build.
COPY src/ src/
RUN dotnet publish src/TradingAI.API/TradingAI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Railway / Fly / Render set $PORT — bind Kestrel to it. Default 8080 for local docker run.
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "TradingAI.API.dll"]
