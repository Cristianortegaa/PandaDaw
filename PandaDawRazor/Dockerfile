FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ── Etapa Node: compilar Tailwind CSS ──
FROM node:20-alpine AS tailwind
WORKDIR /src
COPY PandaDawRazor/package.json PandaDawRazor/package-lock.json* ./
RUN npm install
COPY PandaDawRazor/tailwind.config.js ./
COPY PandaDawRazor/wwwroot/css/input.css ./wwwroot/css/input.css
COPY PandaDawRazor/Pages ./Pages
RUN npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css --minify

# ── Etapa Build: compilar .NET ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PandaDawRazor/PandaDawRazor.csproj", "PandaDawRazor/"]
RUN dotnet restore "PandaDawRazor/PandaDawRazor.csproj"
COPY . .
# Copiar el CSS generado por Tailwind
COPY --from=tailwind /src/wwwroot/css/output.css /src/PandaDawRazor/wwwroot/css/output.css
WORKDIR "/src/PandaDawRazor"
RUN dotnet build "./PandaDawRazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PandaDawRazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PandaDawRazor.dll"]
