# ───────────────────────────────────────────────────────────
# Etapa 1: BUILD — SDK completo para restaurar y compilar
#   Contexto de build = carpeta src/ (contiene Catalog y Core)
# ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar SOLO los .csproj y restaurar primero (caché de capas).
# Catalog depende de Core, así que ambos .csproj entran antes del restore.
COPY ["ShopMicro.Catalog/ShopMicro.Catalog.csproj", "ShopMicro.Catalog/"]
COPY ["ShopMicro.Core/ShopMicro.Core.csproj", "ShopMicro.Core/"]
RUN dotnet restore "ShopMicro.Catalog/ShopMicro.Catalog.csproj"

# Ahora el resto del código y compilar.
COPY . .
WORKDIR "/src/ShopMicro.Catalog"
RUN dotnet build "ShopMicro.Catalog.csproj" -c Release -o /app/build

# ───────────────────────────────────────────────────────────
# Etapa 2: PUBLISH — artefactos listos para ejecutar
# ───────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "ShopMicro.Catalog.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false

# ───────────────────────────────────────────────────────────
# Etapa 3: RUNTIME — imagen final, solo runtime ASP.NET Core
# ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Escuchar en HTTP dentro del contenedor (el Gateway habla http hacia dentro).
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ShopMicro.Catalog.dll"]
