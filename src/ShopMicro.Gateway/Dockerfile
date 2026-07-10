# ───────────────────────────────────────────────────────────
# Etapa 1: BUILD — SDK completo para restaurar y compilar el Gateway
#   Contexto de build = carpeta src/
# ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# El Gateway no tiene ProjectReference: basta su propio .csproj para el restore.
COPY ["ShopMicro.Gateway/ShopMicro.Gateway.csproj", "ShopMicro.Gateway/"]
RUN dotnet restore "ShopMicro.Gateway/ShopMicro.Gateway.csproj"

COPY ["ShopMicro.Gateway/", "ShopMicro.Gateway/"]
WORKDIR "/src/ShopMicro.Gateway"
RUN dotnet build "ShopMicro.Gateway.csproj" -c Release -o /app/build

# ───────────────────────────────────────────────────────────
# Etapa 2: PUBLISH
# ───────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "ShopMicro.Gateway.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false

# ───────────────────────────────────────────────────────────
# Etapa 3: RUNTIME — imagen final, solo runtime ASP.NET Core
# ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ShopMicro.Gateway.dll"]
