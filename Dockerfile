# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY Adrians/Adrians.csproj Adrians/
RUN dotnet restore Adrians/Adrians.csproj
COPY . .
WORKDIR /src/Adrians
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# === Runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Adrians.dll"]
