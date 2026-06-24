FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/EnergyMix.API.csproj src/
RUN dotnet restore src/EnergyMix.API.csproj

COPY src/ ./src/
RUN dotnet publish src/EnergyMix.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EnergyMix.API.dll"]
