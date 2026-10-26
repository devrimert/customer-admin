# Build stage: the full SDK, only used to compile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until the project file changes
COPY CustomerAdmin.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage: only the ASP.NET runtime, much smaller
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Npgsql looks for the Kerberos library on startup and logs an error if it's missing
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

# The image comes with a non-root user, and listens on 8080 by default
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CustomerAdmin.Api.dll"]
