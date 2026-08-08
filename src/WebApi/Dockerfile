# Genocs.Fonet PDF Web API
# Multi-stage build for linux/amd64. Templates, fonts, and assets are expected
# on external volumes (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY fonet.slnx ./
COPY src/Genocs.Fonet/Genocs.Fonet.csproj src/Genocs.Fonet/
COPY src/Genocs.Fonet.XsltTransformer/Genocs.Fonet.XsltTransformer.csproj src/Genocs.Fonet.XsltTransformer/
COPY src/WebApi/Host.csproj src/WebApi/

RUN dotnet restore src/WebApi/Host.csproj

COPY src/Genocs.Fonet/ src/Genocs.Fonet/
COPY src/Genocs.Fonet.XsltTransformer/ src/Genocs.Fonet.XsltTransformer/
COPY src/WebApi/ src/WebApi/

RUN dotnet publish src/WebApi/Host.csproj -c Debug -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# SkiaSharp / fontconfig native dependencies for Linux PDF rendering
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        fontconfig \
        fonts-dejavu-core \
        libfontconfig1 \
        libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

# External volume mount points (templates, fonts, assets)
RUN mkdir -p /app/data/templates /app/data/fonts /app/data/assets \
    && chmod -R 755 /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV pdfStorage__templatesPath=/app/data/templates
ENV pdfStorage__fontsPath=/app/data/fonts
ENV pdfStorage__assetsPath=/app/data/assets

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Genocs.Fonet.WebApi.dll"]
