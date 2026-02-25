# PandaDaw - Playwright E2E Tests (C# / NUnit)

## 📋 Descripción

Tests End-to-End (E2E) para la aplicación PandaDaw usando **Playwright** con **C# 14** y **NUnit**.  
Cubren todas las vistas de la aplicación: Login, Register, Index, Detalle, Carrito, Favoritos, Pago, Pedidos, AdminPanel, Logout y Error.

## 🏗️ Estructura

```
PandaDaw-Playwright/
├── Config/
│   ├── BaseTest.cs              # Clase base: configuración browser, contexto, evidencias
│   └── TestConstants.cs         # URLs, credenciales, rutas centralizadas
├── Extensions/
│   └── PlaywrightExtensions.cs  # Funciones de extensión para sintaxis fluida
├── Tests/
│   ├── LayoutTests.cs           # Navbar, footer, tema, modal logout
│   ├── LoginTests.cs            # Formulario login, validaciones, toggle password
│   ├── RegisterTests.cs         # Formulario registro, validaciones, registro exitoso
│   ├── IndexTests.cs            # Catálogo, búsqueda, filtros categoría, productos
│   ├── DetalleTests.cs          # Detalle producto, carrito, favorito, valoraciones
│   ├── CarritoTests.cs          # Líneas, cantidad, vaciar, resumen, enlace pago
│   ├── FavoritosTests.cs        # Lista favoritos, eliminar, añadir al carrito
│   ├── PagoTests.cs             # Checkout, métodos pago, tarjeta, confirmación
│   ├── PedidosTests.cs          # Historial, estadísticas, estados
│   ├── AdminPanelTests.cs       # CRUD productos, filtros, modales, protección admin
│   ├── LogoutTests.cs           # Cerrar sesión, protección rutas
│   ├── ErrorTests.cs            # Página error, rutas inexistentes
│   ├── FlujosCompletosTests.cs  # Flujos E2E completos de usuario real
│   └── ResilienciaTests.cs      # Técnicas avanzadas: network, DOM, responsive
├── results/                     # 📁 Carpeta de resultados (generada automáticamente)
│   ├── screenshots/             # Capturas de pantalla (PNG)
│   ├── videos/                  # Grabaciones de tests (WebM)
│   └── traces/                  # Trazas Playwright (ZIP)
├── .runsettings                 # Config por defecto (Chromium)
├── chromium.runsettings         # Config para Chromium
├── firefox.runsettings          # Config para Firefox
├── webkit.runsettings           # Config para WebKit (Safari)
├── run-all-browsers.ps1         # Script para ejecutar en TODOS los navegadores
└── PandaDaw-Playwright.csproj   # Proyecto NUnit
```

## 🚀 Instalación

```bash
# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Compilar (genera el script playwright.ps1)
dotnet build

# 3. Instalar los tres navegadores (Chromium, Firefox, WebKit)
pwsh bin/Debug/net10.0/playwright.ps1 install --with-deps
```

## ▶️ Ejecución

### Un solo navegador (Chromium por defecto)
```bash
dotnet test
```

### Navegador específico
```bash
dotnet test --settings:chromium.runsettings
dotnet test --settings:firefox.runsettings
dotnet test --settings:webkit.runsettings
```

### Todos los navegadores (Chromium + Firefox + WebKit)
```powershell
.\run-all-browsers.ps1
```

## 📊 Resultados

Tras la ejecución, los resultados se encuentran en `results/`:

| Tipo | Ruta | Formato |
|------|------|---------|
| **Screenshots** | `results/screenshots/` | PNG (fullPage) |
| **Videos** | `results/videos/` | WebM |
| **Trazas** | `results/traces/` | ZIP |

### Ver trazas con Playwright Inspector
```bash
npx playwright show-trace results/traces/NombreDelTest.zip
```

## 🛡️ Técnicas de Resiliencia

- **Network Interception:** Bloqueo de Google Analytics, ads y trackers
- **DOM Scrubbing:** Limpieza quirúrgica de banners y popups
- **Network Idle:** Espera a silencio de red (sin `Thread.sleep`)
- **Force Click:** Bypass de overlays (cookies, banners)
- **Extension Methods:** `PlaywrightExtensions.cs` con `TestId()`, `ById()`, `ByName()`, etc.

## ⚙️ Configuración

Edita `Config/TestConstants.cs` para cambiar:
- `BaseUrl` → URL de la aplicación
- Credenciales de usuarios seed
- Rutas de la aplicación

Edita `Config/BaseTest.cs` para cambiar:
- `Headless` → `false` para ver el navegador en vivo
- `SlowMo` → milisegundos de pausa entre acciones
- `ViewportSize` → resolución de pantalla
- `Locale` / `TimezoneId` → idioma y zona horaria
