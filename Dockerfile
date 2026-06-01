# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Adrians/Adrians.csproj Adrians/
RUN dotnet restore Adrians/Adrians.csproj

COPY . .
WORKDIR /src/Adrians
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# === Runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Adrians.dll"]