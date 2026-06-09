# Módulo 3 — Desarrollo de Microservicios con ASP.NET Core

> **Duración:** ~5 horas  
> **Versión base:** .NET 8 LTS  
> **Lenguaje:** C#  
> **Arquitectura:** Vertical Slice + MediatR + FluentValidation  
> **Proyecto hilo conductor:** ShopMicro → Catalog.API

---

## 📋 Índice de lecciones

| Lección | Tema | Duración | Conceptos clave |
|---------|------|----------|-----------------|
| **3.1** | Diseño de API REST | 45 min | Recursos, verbos HTTP, códigos de estado, versionado |
| **3.2** | Construcción de Catalog.API | 90 min | Vertical Slice, Nested Classes, MediatR, ISP |
| **3.3** | Capas, DTOs y Mapeo | 60 min | Arquitectura por capas, DTOs, AutoMapper, excepciones |
| **3.4** | Validación con FluentValidation | 60 min | Validadores declarativos, cross-field, async, testing |
| **3.5** | Manejo de errores (ProblemDetails) | 45 min | RFC 7807, excepciones de dominio, middleware global |
| **3.6** | Documentación (OpenAPI/Swagger) | 45 min | Swagger UI, comentarios XML, versionado API |
| **3.7** | Health Checks | 30 min | Liveness/Readiness, checks predefinidos, custom checks |

**Total:** ~5 horas (376 minutos)

---

## 🎯 Objetivos del módulo

Al completar este módulo serás capaz de:

✅ Diseñar APIs REST siguiendo principios HATEOAS  
✅ Implementar servicios con arquitectura **Vertical Slice** (feature-driven)  
✅ Usar **MediatR** como patrón de mediador para desacoplamiento  
✅ Validar entrada robustamente con **FluentValidation**  
✅ Manejar errores consistentemente con **ProblemDetails** (RFC 7807)  
✅ Documentar APIs automáticamente con **Swagger/OpenAPI**  
✅ Implementar health checks para monitoreo en Kubernetes  
✅ Aplicar **SOLID** (especialmente ISP en repositorios)  
✅ Separar responsabilidades con DTOs y mapeo  
✅ Construir Catalog.API funcional y lista para producción  

---

## 🏗️ Arquitectura implementada

### Estructura de carpetas (Vertical Slice)

```
Catalog.API/
├── Features/
│   └── Products/
│       ├── Create/
│       │   ├── CreateProduct.cs        (Endpoint + Handler + Command + Response)
│       │   ├── CreateProductValidator.cs
│       │   └── CreateProductRequest.cs
│       ├── GetById/
│       │   └── GetProductById.cs       (Query versión del patrón)
│       ├── List/
│       │   └── ListProducts.cs
│       ├── Update/
│       ├── Delete/
│       ├── Domain/
│       │   ├── Product.cs              (Entidad de dominio)
│       │   ├── IProductRepository.cs
│       │   └── Exceptions/
│       │       ├── ProductNotFoundException.cs
│       │       ├── InsufficientStockException.cs
│       │       └── InvalidProductException.cs
│       ├── Infrastructure/
│       │   ├── ProductRepository.cs    (Implementación EF Core)
│       │   └── CatalogDbContext.cs
│       └── Dtos/
│           ├── ProductResponse.cs
│           ├── ProductListItemResponse.cs
│           └── CreateProductRequest.cs
├── Core/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs       (Pipeline de MediatR)
│   ├── Localization/
│   ├── Validators/
│   │   └── CustomValidators.cs         (Extensiones reutilizables)
│   └── Errors/
│       └── ErrorTypes.cs
├── Infrastructure/
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs
│   │   └── ValidationExceptionHandler.cs
│   └── HealthChecks/
│       └── CustomHealthCheck.cs
├── Program.cs
└── appsettings.json
```

### Flujo de una solicitud

```
HTTP Request
    ↓
Endpoint (ControllerBase)
    ↓
MediatR.Send(Command/Query)
    ↓
ValidationBehavior (Pipeline)
  ├─ FluentValidation ejecuta validadores
  └─ Si fallos: lanza ValidationException
    ↓
Handler (IRequestHandler<T, R>)
    ├─ Accede a IProductRepository (ISP)
    └─ Devuelve Response
    ↓
GlobalExceptionMiddleware (captura excepciones)
    ├─ Transforma en ProblemDetails (RFC 7807)
    └─ Retorna JSON con status HTTP apropiado
    ↓
HTTP Response (200, 201, 400, 404, 409, 422, 500, etc.)
```

---

## 🔧 Tecnologías utilizadas

| Componente | Tecnología | Función |
|------------|-----------|---------|
| **API Framework** | ASP.NET Core Minimal API | Endpoints HTTP |
| **ORM** | Entity Framework Core | Acceso a datos |
| **Base de datos** | PostgreSQL (recomendado) | Persistencia |
| **Patrón de mediador** | MediatR | Desacoplamiento |
| **Validación** | FluentValidation | Validación declarativa |
| **Mapeo** | AutoMapper (optional) | DTO ↔ Entity |
| **Documentación** | Swagger/OpenAPI | API autodescriptiva |
| **Health Checks** | AspNetCore.HealthChecks | Monitoreo K8s |
| **Logging** | ILogger (built-in) | Trazabilidad |

---

## 📝 Patrones implementados

### 1. Vertical Slice Architecture
**Beneficio:** Cambios localizados, menos conflictos merge  
**Ejemplo:** Feature CreateProduct = 1 carpeta con Endpoint, Handler, Validator, Request, Response

### 2. Nested Classes
**Beneficio:** Cohesión alta, encapsulación  
**Ejemplo:**
```csharp
public static class CreateProduct
{
    public class Endpoint : ControllerBase { ... }
    public class Handler : IRequestHandler { ... }
    public class Validator : AbstractValidator { ... }
    public record Command { ... }
    public record Response { ... }
}
```

### 3. CQRS Lite (Command Query Responsibility Segregation)
**Beneficio:** Separación clara entre operaciones lectura/escritura  
**Ejemplo:**
- Commands: CreateProduct, UpdateProduct, DeleteProduct
- Queries: GetProductById, ListProducts

### 4. Interface Segregation Principle (ISP)
**Beneficio:** Interfaces específicas, no genéricas  
**Ejemplo:**
```csharp
public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken ct);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Product product, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
```

### 5. Problem Details (RFC 7807)
**Beneficio:** Respuestas de error consistentes y machine-readable  
**Ejemplo:**
```json
{
  "type": "https://api.shopmicro.local/errors/validation-error",
  "title": "Validation Error",
  "status": 422,
  "detail": "...",
  "errors": { "Name": [...], "Price": [...] }
}
```

---

## 🚀 Flujo de desarrollo práctico

### Paso 1: Definir el recurso (Dominio)
```csharp
public class Product { ... }
public interface IProductRepository { ... }
```

### Paso 2: Crear la feature (Command/Query + Handler)
```csharp
public static class CreateProduct
{
    public class Handler : IRequestHandler<Command, Response> { ... }
    public record Command(...) : IRequest<Response>;
    public record Response(...);
}
```

### Paso 3: Validar entrada
```csharp
public class CreateProductValidator : AbstractValidator<CreateProduct.Command>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        // ... más reglas
    }
}
```

### Paso 4: Exponer el endpoint
```csharp
[HttpPost]
[ProduceResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
[ProduceResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
{
    var command = request.ToCommand();
    var response = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}
```

### Paso 5: Documentar
```csharp
/// <summary>Crea un nuevo producto</summary>
/// <param name="request">Datos del producto</param>
/// <returns>Producto creado</returns>
/// <response code="201">Producto creado exitosamente</response>
/// <response code="422">Error de validación</response>
```

---

## 🎓 Concepto central: Vertical Slice vs. Capas tradicionales

### ❌ Capas tradicionales (difícil de mantener)

```
Controllers/
  ProductController.cs
Services/
  ProductService.cs
Repositories/
  ProductRepository.cs
Models/
  Product.cs
  CreateProductDto.cs
  CreateProductResponse.cs

Para agregar una feature: tocar 5 archivos en 5 carpetas
```

### ✅ Vertical Slice (fácil de mantener)

```
Features/Products/Create/
  CreateProductEndpoint.cs
  CreateProductHandler.cs
  CreateProductValidator.cs
  CreateProductRequest.cs

Para agregar una feature: nueva carpeta = cambios localizados
```

---

## 📚 Referencia de comandos útiles

### Crear proyecto
```bash
dotnet new webapi -n Catalog.API -f net8.0
cd Catalog.API
```

### Instalar dependencias
```bash
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package Swashbuckle.AspNetCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Ejecutar
```bash
dotnet build
dotnet run
# Swagger UI: https://localhost:5001 (Development)
# Health: https://localhost:5001/health/ready
```

### Crear migración (EF Core)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## ✅ Checklist de finalización del módulo

Antes de pasar al Módulo 4, verifica:

- [ ] Comprendido Vertical Slice Architecture
- [ ] Implementado al menos 3 features CRUD (Create, Read, Update)
- [ ] Todos los endpoints validados con FluentValidation
- [ ] Errores manejados con ProblemDetails (RFC 7807)
- [ ] Documentación Swagger generada automáticamente
- [ ] Health checks implementados (liveness + readiness)
- [ ] Base de datos PostgreSQL integrada
- [ ] Repositorio sigue ISP (métodos específicos, no genéricos)
- [ ] MediatR configurado con ValidationBehavior
- [ ] DTOs separados de entidades de dominio
- [ ] Tests unitarios para validadores

---

## 🔗 Conexión con módulos siguientes

### Módulo 4 — Comunicación entre Microservicios
Con Catalog.API completa, ahora necesitará **comunicarse**:
- HTTP con HttpClientFactory
- Refit (typed clients)
- gRPC (alto rendimiento)
- Mensajería asíncrona (RabbitMQ + MassTransit)

### Módulo 5 — Seguridad
Catalog.API necesitará **autenticación y autorización**:
- JWT Bearer tokens
- OAuth2 / OpenID Connect
- Autorización por roles y políticas

### Módulo 6 — Gestión de Datos
Más adelante, escalaremos **persistencia**:
- Database per service
- Saga pattern para transacciones distribuidas
- Outbox pattern para consistencia

### Módulo 7 — Observabilidad y Despliegue
Finalmente, operacionalizaremos:
- OpenTelemetry + Jaeger (trazas distribuidas)
- Docker + Docker Compose
- Kubernetes (pods, deployments, services)

---

## 📖 Recursos adicionales

**Documentación oficial:**
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [OpenAPI / Swagger](https://swagger.io/)

**Comunidad:**
- [.NET Discord](https://discord.gg/dotnet)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/asp.net-core)

---

## 🎯 Conclusión

Has aprendido a construir **microservicios empresariales en .NET** siguiendo:

1. **REST puro:** Recursos, verbos HTTP, códigos correctos
2. **Arquitectura limpia:** Vertical Slice, separación de responsabilidades
3. **Validación robusta:** FluentValidation con reglas complejas
4. **Manejo de errores:** ProblemDetails estándar (RFC 7807)
5. **Documentación automática:** Swagger/OpenAPI
6. **Monitoreo:** Health checks para Kubernetes
7. **Principios SOLID:** Especialmente ISP y SRP

**Catalog.API está lista para:**
- Ser consumida por otros servicios (Módulo 4)
- Ser asegurada (Módulo 5)
- Escalar (Módulo 6)
- Ser desplegada en producción (Módulo 7)

---

**Próximo módulo:** [4 — Comunicación entre Microservicios]
