# Genocs.Fonet PDF Web API
# Multi-stage build for linux/amd64.
# Templates, fonts, and assets are expected on external volumes (see docker-compose.yml).

# Use the official .NET runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
ARG BUILD_CONFIGURATION=Debug
WORKDIR /app
EXPOSE 8080

# SkiaSharp native lib (libSkiaSharp.so) depends on fontconfig/freetype at runtime.
# These must be installed in the final aspnet image, not only in the SDK build stage.
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        fontconfig \
        fonts-dejavu-core \
        libfontconfig1 \
        libfreetype6 \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/data/templates /app/data/fonts /app/data/assets \
    && chown -R app:app /app
USER app

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Copy global build configuration files first for better layer caching
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["stylecop.json", "stylecop.json"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["global.json", "global.json"]

# COPY .csproj and restore as distinct layers
# This is CRITICAL: If code changes but .csproj doesn't, Docker reuses this cached layer
COPY src/Fonet/Fonet.csproj src/Fonet/
COPY src/XsltTransformer/XsltTransformer.csproj src/XsltTransformer/
COPY src/WebApi/Host.csproj src/WebApi/
RUN dotnet restore src/WebApi/Host.csproj

# Copy the rest of the source code
COPY src/WebApi/ src/WebApi/
COPY src/Fonet/ src/Fonet/
COPY src/XsltTransformer/ src/XsltTransformer/

WORKDIR "/src/src/WebApi"
RUN dotnet build "Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV pdfStorage__templatesPath=/app/data/templates
ENV pdfStorage__fontsPath=/app/data/fonts
ENV pdfStorage__assetsPath=/app/data/assets

ENTRYPOINT ["dotnet", "Genocs.Fonet.WebApi.dll"]
