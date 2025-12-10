# === Build Stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore packages
RUN dotnet restore ./Payona.API.csproj

# Build
RUN dotnet publish ./Payona.API.csproj -c Release -o /app/publish

# === Runtime Stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published artifacts
COPY --from=build /app/publish .

# Expose API port (Render uses 10000 internally)
EXPOSE 8080

# Render requires ASPNETCORE_URLS set exactly like this:
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# RUN
ENTRYPOINT ["dotnet", "Payona.API.dll"]
