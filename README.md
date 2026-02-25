<!-- markdownlint-disable MD033 MD041 -->

<div align="center">

![PandaDaw Logo](https://github.com/user-attachments/assets/95b56152-4356-499b-b955-6cea60754ff8)

# PandaDaw 🐼

### E-commerce Moderno de Productos Tecnológicos

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=.net)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-10.0-239120?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat&logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D?style=flat&logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat)](LICENSE)

**E-commerce completo desarrollado con ASP.NET Core 10, Razor Pages y arquitectura limpia.**

[Demo](#demo) • [Características](#características) • [Tecnologías](#tecnologías) • [Primeros Pasos](#primeros-pasos) • [Estructura](#estructura) • [API](#api) • [Contribuir](#contribuir)

</div>

---

## 📌 Acerca del Proyecto

**PandaDaw** es una plataforma de comercio electrónico moderna y completa para la venta de productos tecnológicos. Construido con las últimas tecnologías de Microsoft, ofrece una experiencia de usuario fluida tanto para clientes como para administradores.

### 🎯 Propósito

- Proporcionar una tienda online funcional y escalable
- Demostrar arquitectura de software limpia con .NET 10
- Showcase de buenas prácticas de desarrollo

---

## ✨ Características

### 👥 Para Usuarios

- 🔐 **Autenticación Segura** - Registro, login y gestión de cuentas con JWT
- 🛒 **Carrito de Compras** - Añadir, modificar y eliminar productos
- ❤️ **Lista de Favoritos** - Guardar productos para después
- ⭐ **Valoraciones y Reseñas** - Opinar sobre productos comprados
- 📦 **Seguimiento de Pedidos** - Historial y estado de compras
- 🔍 **Exploración por Categorías** - Audio, Imagen, Smartphones, Laptops, Gaming

### 👨‍💻 Para Administradores

- 📊 **Panel de Administración** - Gestión completa de productos
- 📈 **Gestión de Pedidos** - Control del estado de cada venta
- 👥 **Gestión de Usuarios** - Control de roles y permisos

### 🏗️ Técnicas

- 🗄️ **Base de Datos PostgreSQL** - Persistencia robusta
- ⚡ **Cache con Redis** - Alto rendimiento
- 🛡️ **Rate Limiting** - Protección contra abusos
- 📝 **Validaciones** - FluentValidation para datos
- 🧪 **Testing** - Unit tests e integración
- 📦 **Docker** - Despliegue contenerizado
- 📡 **API REST** - Arquitectura RESTful

---

## 🛠️ Tecnologías

### Backend

| Tecnología | Propósito |
|-------------|-----------|
| **.NET 10** | Framework principal |
| **ASP.NET Core** | Web API y servicios |
| **Entity Framework Core** | ORM para PostgreSQL |
| **PostgreSQL 16** | Base de datos relacional |
| **Redis 7** | Cache y sesiones |
| **JWT** | Autenticación stateless |
| **FluentValidation** | Validación de modelos |
| **Serilog** | Logging estructurado |
| **AspNetCoreRateLimit** | Rate limiting |

### Frontend

| Tecnología | Propósito |
|-------------|-----------|
| **Razor Pages** | Framework web |
| **Tailwind CSS 4** | Estilos modernos |
| **DaisyUI 5** | Componentes UI |

### Infraestructura

| Tecnología | Propósito |
|-------------|-----------|
| **Docker** | Contenedores |
| **Docker Compose** | Orquestación |
| **GitHub Actions** | CI/CD |

---

## 🚀 Primeros Pasos

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL 16](https://www.postgresql.org/download/) (opcional, local)
- [Node.js 20+](https://nodejs.org/) (para desarrollo frontend)

### Instalación con Docker (Recomendado)

```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/PandaDaw.git
cd PandaDaw

# 2. Iniciar los servicios con Docker Compose
docker-compose up -d

# 3. Acceder a los servicios
# - Frontend: http://localhost:5062
# - Backend API: http://localhost:5088
# - PostgreSQL: localhost:5434
```

### Instalación Local

```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/PandaDaw.git
cd PandaDaw

# 2. Configurar variables de entorno
# Crear archivo PandaBack/appsettings.Development.json

# 3. Restaurar dependencias
dotnet restore

# 4. Ejecutar migraciones (desde PandaBack)
dotnet ef database update

# 5. Ejecutar la aplicación
dotnet run --project PandaBack
dotnet run --project PandaDawRazor
```

### Variables de Entorno

```env
# Connection Strings
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=pandadb;Username=panda;Password=tu_password"
ConnectionStrings__Redis="localhost:6379,password=tu_redis_password"

# JWT
Jwt__Key="TuClaveSecretaMuyLargaParaSeguridad"
Jwt__Issuer="PandaBack"
Jwt__Audience="PandaFront"
Jwt__ExpireInMinutes=120
```

---

## 📂 Estructura del Proyecto

```
PandaDaw/
├── 📁 PandaBack/                    # Backend API
│   ├── 🗂️ Controllers/              # Controladores API
│   ├── 🗂️ Services/                 # Lógica de negocio
│   ├── 🗂️ Repositories/              # Acceso a datos
│   ├── 🗂️ Models/                    # Entidades EF Core
│   ├── 🗂️ DTOs/                      # Data Transfer Objects
│   ├── 🗂️ Mappers/                   # Mapping DTOs <-> Models
│   ├── 🗂️ Validators/               # FluentValidation
│   ├── 🗂️ Middleware/                # Custom middleware
│   ├── 🗂️ Data/                      # DbContext y seeders
│   └── 🗀️ Dockerfile
│
├── 📁 PandaDawRazor/                # Frontend Razor Pages
│   ├── 🗂️ Pages/                     # Razor Pages
│   ├── 🗂️ wwwroot/                   # Static files
│   └── 🗀️ Dockerfile
│
├── 📁 Tests/                        # Proyecto de tests
│   ├── 🗂️ Controllers/              # Tests de controladores
│   ├── 🗂️ Services/                 # Tests de servicios
│   ├── 🗂️ Integration/              # Tests de integración
│   ├── 🗂️ Validators/               # Tests de validaciones
│   ├── 🗂️ Mappers/                  # Tests de mapeos
│   └── 🗂️ Models/                   # Tests de modelos
│
├── 📄 compose.yaml                  # Docker Compose
├── 📄 PandaDaw.sln                  # Solución .NET
└── 📄 README.md
```

---

## 📡 API Endpoints

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar usuario |
| POST | `/api/auth/login` | Iniciar sesión |
| GET | `/api/auth/me` | Datos del usuario actual |

### Productos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/productos` | Listar todos los productos |
| GET | `/api/productos/{id}` | Obtener producto por ID |
| POST | `/api/productos` | Crear producto (Admin) |
| PUT | `/api/productos/{id}` | Actualizar producto (Admin) |
| DELETE | `/api/productos/{id}` | Eliminar producto (Admin) |

### Carrito

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/carrito` | Obtener carrito del usuario |
| POST | `/api/carrito/add` | Añadir producto al carrito |
| PUT | `/api/carrito/update` | Actualizar cantidad |
| DELETE | `/api/carrito/remove/{lineaId}` | Eliminar línea del carrito |

### Favoritos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/favoritos` | Listar favoritos del usuario |
| POST | `/api/favoritos` | Añadir a favoritos |
| DELETE | `/api/favoritos/{id}` | Eliminar de favoritos |

### Valoraciones

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/valoraciones/producto/{id}` | Valoraciones de un producto |
| POST | `/api/valoraciones` | Crear valoración |
| DELETE | `/api/valoraciones/{id}` | Eliminar valoración |

### Ventas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/ventas` | Listar ventas del usuario |
| GET | `/api/ventas/{id}` | Detalles de una venta |
| POST | `/api/ventas` | Crear nueva venta |
| PUT | `/api/ventas/{id}/estado` | Actualizar estado (Admin) |

> 📌 Documentación completa disponible en `/swagger` cuando el servidor está en ejecución.

---

## 🧪 Testing

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests específicos
dotnet test --filter "FullyQualifiedName~Tests.Services"
```

---

## 📦 Despliegue

### Producción con Docker

```bash
# Build de las imágenes
docker-compose build

# Iniciar servicios
docker-compose up -d

# Ver logs
docker-compose logs -f
```

### Servicios en Producción

| Servicio | Puerto | Descripción |
|----------|--------|-------------|
| Frontend | 5062 | Interfaz de usuario |
| Backend API | 5088 | API REST |
| PostgreSQL | 5434 | Base de datos |
| Redis | - | Cache (interno) |

---

## 🤝 Contribuir

1. **Fork** el repositorio
2. Crear una rama (`git checkout -b feature/nueva-caracteristica`)
3. **Commit** tus cambios (`git commit -m 'Add: nueva característica'`)
4. **Push** a la rama (`git push origin feature/nueva-caracteristica`)
5. Abrir un **Pull Request**

### Buenas Prácticas

- ✅ Sigue el estilo de código existente
- ✅ Añade tests para nuevas funcionalidades
- ✅ Actualiza la documentación
- ✅ Usa mensajes de commit descriptivos

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - voir el archivo [LICENSE](LICENSE) para más detalles.

---

## 🙏 Agradecimientos

- [Microsoft](https://www.microsoft.com/) por .NET
- [Community](https://docs.microsoft.com/en-us/aspnet/core/) por la documentación
- [Contribuidores](https://github.com/tu-usuario/PandaDaw/graphs/contributors)

---

<div align="center">

**⭐ ¡Dale una estrella al proyecto si te fue útil!**

Hecho con ❤️ por [PandaDaw Team](https://github.com/tu-usuario)

</div>
