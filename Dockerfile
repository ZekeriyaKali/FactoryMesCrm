# ==========================================
# 1. BUILD / SDK STAGE
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Docker cache mekanizmasýndan maksimum yararlanmak için önce csproj dosyalarýný kopyalýyoruz
COPY ["src/Core/FactoryMesCrm.Domain/FactoryMesCrm.Domain.csproj", "src/Core/FactoryMesCrm.Domain/"]
COPY ["src/Core/FactoryMesCrm.Application/FactoryMesCrm.Application.csproj", "src/Core/FactoryMesCrm.Application/"]
COPY ["src/Infrastructure/FactoryMesCrm.Persistence/FactoryMesCrm.Persistence.csproj", "src/Infrastructure/FactoryMesCrm.Persistence/"]
COPY ["src/Infrastructure/FactoryMesCrm.Infrastructure/FactoryMesCrm.Infrastructure.csproj", "src/Infrastructure/FactoryMesCrm.Infrastructure/"]
COPY ["src/Presentation/FactoryMesCrm.Api/FactoryMesCrm.Api.csproj", "src/Presentation/FactoryMesCrm.Api/"]

# Baðýmlýlýklarý restore et
RUN dotnet restore "src/Presentation/FactoryMesCrm.Api/FactoryMesCrm.Api.csproj"

# Tüm kaynak kodlarý kopyala ve derle
COPY . .
WORKDIR "/src/src/Presentation/FactoryMesCrm.Api"
RUN dotnet publish "FactoryMesCrm.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# 2. RUNTIME STAGE (Non-Root Production)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Güvenlik Sýkýlaþtýrmasý: Container'ý root olmayan varsayýlan 'app' kullanýcýsý ile çalýþtýr
USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "FactoryMesCrm.Api.dll"]