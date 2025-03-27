# Phase 1 : image runtime ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Phase 2 : build et restore
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# On copie les .csproj de chaque sous-projet
COPY ["Patient.API/Patient.API.csproj", "Patient.API/"]
COPY ["Patient.Core/Patient.Core.csproj", "Patient.Core/"]
COPY ["Patient.Data/Patient.Data.csproj", "Patient.Data/"]
COPY ["Patient.Models/Patient.Models.csproj", "Patient.Models/"]
# => Ajoutez les copies pour tous les sous-projets nécessaires

RUN dotnet restore "Patient.API/Patient.API.csproj"

# On copie tout le code restant
COPY . .
WORKDIR "/src/Patient.API"
RUN dotnet build "Patient.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Phase 3 : publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Patient.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Phase 4 : image finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Patient.API.dll"]
