# Base Stage - Minimal Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine-amd64 AS base

# Install only essential dependencies
RUN apk add --no-cache icu-libs tzdata \
    && rm -rf /var/cache/apk/* /tmp/* /var/tmp/*

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser

WORKDIR /app
EXPOSE 8080

# Restore Stage - Dependencies Only
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine-amd64 AS restore

WORKDIR /src

# Copy only project files for layer caching
COPY *.sln ./
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

# Restore with minimal output
RUN dotnet restore "Kickify.Api/Kickify.Api.csproj" \
    --locked-mode \
    --runtime alpine-x64 \
    --verbosity quiet

# Build Stage
FROM restore AS build

ARG BUILD_CONFIGURATION=Release

# Copy source code
COPY . .
WORKDIR "/src/Kickify.Api"

# Build with optimization flags
RUN dotnet build "Kickify.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore \
    --runtime alpine-x64 \
    /p:TreatWarningsAsErrors=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

# Publish Stage
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

# Publish with maximum optimizations
RUN dotnet publish "Kickify.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --no-build \
    --runtime alpine-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:DebugType=None \
    /p:DebugSymbols=false \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=false \
    /p:EnableCompressionInSingleFile=true \
    /p:TrimMode=link \
    /p:IlcOptimizationPreference=Size \
    /p:IlcGenerateStackTraceData=false \
    && find /app/publish -type f \( -name "*.pdb" -o -name "*.xml" \) -delete \
    && find /app/publish -type f -name "*Development.json" -delete \
    && rm -rf /app/publish/refs \
    && rm -rf /app/publish/runtimes/*/native/*.a

# Final Stage - Ultra Lightweight
FROM base AS final

USER appuser
WORKDIR /app

# Copy only published files
COPY --from=publish --chown=appuser:appuser /app/publish .

# Minimal environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0 \
    COMPlus_EnableDiagnostics=0 \
    ASPNETCORE_HTTP_PORTS=8080

# Lightweight health check without wget
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD dotnet --info > /dev/null 2>&1 || exit 1

ENTRYPOINT ["dotnet", "Kickify.Api.dll"]