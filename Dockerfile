# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081

# Copy the certificate inside the container
#COPY certs/cert.pfx /https/cert.pfx
#
## Install environmental variables for ASP.NET Core https
#ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx
#ENV ASPNETCORE_Kestrel__Certificates__Default__Password=P@ssword


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CustodialWallet.csproj", "."]
RUN dotnet restore "./CustodialWallet.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./CustodialWallet.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CustodialWallet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Install PostgreSQL client utilities and curl (optional, useful for debugging/healthchecks)
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

# Basic healthcheck to ensure the app has started
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 CMD curl -fsS http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "CustodialWallet.dll"]