# ── Trin 1: base – det lille image, API'et skal KØRE i ──
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ── Trin 2: build – SDK'et, bruges kun til at bygge ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Kun projektfilerne først (så restore kan caches)
COPY Airnb.API/Airnb.API.csproj                       Airnb.API/
COPY Airnb.Application/Airnb.Application.csproj       Airnb.Application/
COPY Airnb.Infrastructure/Airnb.Infrastructure.csproj Airnb.Infrastructure/
COPY Airnb.Domain/Airnb.Domain.csproj                 Airnb.Domain/
COPY Airnb.Shared/Airnb.Shared.csproj                 Airnb.Shared/

RUN dotnet restore "Airnb.API/Airnb.API.csproj"

# 2) Derefter al koden
COPY Airnb.API/            Airnb.API/
COPY Airnb.Application/    Airnb.Application/
COPY Airnb.Infrastructure/ Airnb.Infrastructure/
COPY Airnb.Domain/         Airnb.Domain/
COPY Airnb.Shared/         Airnb.Shared/

RUN dotnet build "Airnb.API/Airnb.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ── Trin 3: publish – de færdige .dll-filer ──
FROM build AS publish
RUN dotnet publish "Airnb.API/Airnb.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ── Trin 4: final – base + kun de færdige filer ──
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Airnb.API.dll"]