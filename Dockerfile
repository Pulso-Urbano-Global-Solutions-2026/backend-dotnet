# ── build stage ────────────────────────────────────────────────────────────────
# Adapted from CONTEXT.md spec: net10.0 requires sdk:10.0 / aspnet:10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy solution + csproj first to leverage layer caching on restore
COPY PulsoUrbano.Net.slnx .
COPY PulsoUrbano.Net/PulsoUrbano.Net.csproj PulsoUrbano.Net/
RUN dotnet restore PulsoUrbano.Net/PulsoUrbano.Net.csproj

# Copy source and publish
COPY PulsoUrbano.Net/ PulsoUrbano.Net/
RUN dotnet publish PulsoUrbano.Net/PulsoUrbano.Net.csproj \
    -c Release -o /app/publish --no-restore

# ── runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app

# Non-root user (satisfies GS DevOps "not root" rule)
RUN addgroup -S pulso && adduser -S pulso -G pulso

COPY --from=build /app/publish .

USER pulso
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "PulsoUrbano.Net.dll"]
