# 📊 Resumen visual: Arquitectura Vertical Slice + Flujo de solicitudes

---

## 🏗️ Vertical Slice Architecture

```
                    ┌─────────────────────────────────┐
                    │  Catalog.API (ShopMicro)        │
                    └──────────────┬──────────────────┘
                                   │
                ┌──────────────────┴──────────────────┐
                │                                     │
         ┌──────▼──────┐                      ┌──────▼──────┐
         │   Features  │                      │    Core     │
         └──────┬──────┘                      └─────────────┘
                │                                     │
        ┌───────┴────────┐                  ┌────────┴────────┐
        │                │                  │                 │
    ┌───▼─────┐    ┌────▼────┐        ┌────▼────┐    ┌──────▼──────┐
    │ Products│    │ Orders  │        │Behaviors│    │   Errors    │
    └────┬────┘    └─────────┘        └────┬────┘    └─────────────┘
         │                                   │
    ┌────┴─────────────────────┐            │
    │                          │            │
┌───▼──┐ ┌────▼────┐ ┌────────▼───┐ ┌─────▼────────────┐
│Create│ │GetById  │ │   List     │ │ValidationBehavior│
│      │ │         │ │            │ │ (Pipeline)       │
└──────┘ └─────────┘ └────────────┘ └──────────────────┘
│        │Create                     │  Ejecuta validadores
│        │  ├─ Endpoint              │  Transforma ValidationException
│        │  ├─ Handler               │  en 422 + ProblemDetails
│        │  ├─ Validator
│        │  ├─ Command
│        │  └─ Response

CADA FEATURE (Create, GetById, etc.) ES UNA CARPETA AUTOCONTENIDA
                             ↓
            CAMBIOS LOCALIZADOS, SIN CASCADA
```

---

## 🔄 Flujo completo de una solicitud HTTP

```
╔════════════════════════════════════════════════════════════════════════════╗
║ 1️⃣ HTTP REQUEST llega a Catalog.API                                       ║
║    POST /api/v1/products                                                   ║
║    Content-Type: application/json                                          ║
║    Body: {                                                                 ║
║      "name": "Laptop XPS 13",                                              ║
║      "description": "Ultrabook premium",                                   ║
║      "price": 999.99,                                                      ║
║      "stock": 5                                                            ║
║    }                                                                       ║
╚════════════════════════════════════════════════════════════════════════════╝
                                   ↓
╔════════════════════════════════════════════════════════════════════════════╗
║ 2️⃣ ENDPOINT (CreateProductEndpoint.Create)                                ║
║    • Recibe CreateProductRequest (DTO)                                     ║
║    • Transforma a CreateProduct.Command                                    ║
║    • Envía via IMediator.Send(command)                                     ║
╚════════════════════════════════════════════════════════════════════════════╝
                                   ↓
╔════════════════════════════════════════════════════════════════════════════╗
║ 3️⃣ MEDIATR PIPELINE                                                       ║
║    • ValidationBehavior intercepta                                         ║
║    • Ejecuta CreateProductValidator                                        ║
║    • Reglas FluentValidation:                                              ║
║      ✓ Name: NotEmpty + Length(3,100) + Matches regex                     ║
║      ✓ Description: NotEmpty + Length(10,500)                             ║
║      ✓ Price: > 0, <= 999999.99                                           ║
║      ✓ Stock: >= 0, <= 999999                                             ║
║    • Si validación falla → throw ValidationException                       ║
║    • Si validación OK → continúa                                           ║
╚════════════════════════════════════════════════════════════════════════════╝
                                   ↓
╔════════════════════════════════════════════════════════════════════════════╗
║ 4️⃣ HANDLER (CreateProduct.Handler)                                        ║
║    • IProductRepository inyectado (ISP - métodos específicos)              ║
║    • var product = Product.Create(...)                                     ║
║    • await _repository.AddAsync(product)                                   ║
║      └─ EF Core + PostgreSQL                                               ║
║         ├─ INSERT INTO products (...)                                      ║
║         └─ SaveChanges() → transacción confirmada                          ║
║    • return new Response(product.Id, product.Price)                        ║
╚════════════════════════════════════════════════════════════════════════════╝
                                   ↓
╔════════════════════════════════════════════════════════════════════════════╗
║ 5️⃣ ENDPOINT (continuación)                                                ║
║    • Recibe Response de Handler                                            ║
║    • return CreatedAtAction(nameof(GetById), ...)                          ║
║      └─ Status: 201 Created                                                ║
║      └─ Location: /api/v1/products/{id}                                    ║
║      └─ Body: Response JSON                                                ║
╚════════════════════════════════════════════════════════════════════════════╝
                                   ↓
╔════════════════════════════════════════════════════════════════════════════╗
║ 6️⃣ RESPONSE (éxito)                                                       ║
║    HTTP/1.1 201 Created                                                    ║
║    Content-Type: application/json                                          ║
║    Location: /api/v1/products/550e8400-e29b-41d4-a716-446655440000       ║
║                                                                            ║
║    {                                                                       ║
║      "id": "550e8400-e29b-41d4-a716-446655440000",                        ║
║      "price": 999.99                                                      ║
║    }                                                                       ║
╚════════════════════════════════════════════════════════════════════════════╝
```

---

## ❌ Flujo en caso de ERRORES

### Caso A: Validación falla (422 Unprocessable Entity)

```
HTTP Request POST /api/v1/products
  ├─ Name: ""                    ❌ Vacío
  ├─ Price: -10                  ❌ Negativo
  └─ Stock: "abc"                ❌ No es número
                                  ↓
         ValidationBehavior
                ↓
    ValidationException lanzada
                ↓
    GlobalExceptionMiddleware atrapa
                ↓
      ┌─────────────────────────────┐
      │ ProblemDetails (RFC 7807)   │
      ├─────────────────────────────┤
      │ status: 422                 │
      │ type: /errors/validation    │
      │ title: Validation Error     │
      │ detail: "..."               │
      │ errors: {                   │
      │   "Name": [                 │
      │     "El nombre es obligatorio",
      │     "Debe tener 3-100 car..." │
      │   ],                        │
      │   "Price": [                │
      │     "Debe ser > 0"          │
      │   ],                        │
      │   "Stock": [                │
      │     "Debe ser número entero"│
      │   ]                         │
      │ }                           │
      └─────────────────────────────┘
                ↓
        HTTP/1.1 422 Unprocessable Entity
```

### Caso B: Recurso no encontrado (404 Not Found)

```
HTTP Request GET /api/v1/products/00000000-0000-0000-0000-000000000000
                                  ↓
         Handler busca en BD
                ↓
    product = await GetByIdAsync(id)  → null
                ↓
    throw new ProductNotFoundException(id)
                ↓
    GlobalExceptionMiddleware atrapa
                ↓
      ┌──────────────────────────────┐
      │ ProblemDetails (RFC 7807)    │
      ├──────────────────────────────┤
      │ status: 404                  │
      │ type: /errors/not-found      │
      │ title: Not Found             │
      │ detail: "El producto con ID  │
      │          00000000-0000-0000  │
      │          no fue encontrado"  │
      │ instance: /api/v1/products/..│
      └──────────────────────────────┘
                ↓
        HTTP/1.1 404 Not Found
```

### Caso C: Conflicto de negocio (409 Conflict)

```
HTTP Request POST /api/v1/orders/add-item
  Body: {
    "productId": "550e8400-...",
    "quantity": 100              ❌ Solo hay 5 en stock
  }
                ↓
         Handler intenta restar stock
                ↓
    if (product.Stock < quantity)
        throw new InsufficientStockException(...)
                ↓
    GlobalExceptionMiddleware atrapa
                ↓
      ┌──────────────────────────────┐
      │ ProblemDetails (RFC 7807)    │
      ├──────────────────────────────┤
      │ status: 409                  │
      │ type: /errors/insufficient.. │
      │ title: Conflict              │
      │ detail: "Stock insuficiente: │
      │          solicitado 100,     │
      │          disponible 5"       │
      │ productId: "550e8400-..."    │
      │ requestedStock: 100          │
      │ availableStock: 5            │
      └──────────────────────────────┘
                ↓
        HTTP/1.1 409 Conflict
```

---

## 📦 Estructura de directorios implementada

```
Catalog.API/
│
├── Features/
│   └── Products/                          ← VERTICAL SLICE
│       ├── Create/
│       │   ├── CreateProduct.cs           ← Nested Classes
│       │   │   ├─ public class Endpoint
│       │   │   ├─ public class Handler    ← IRequestHandler<Command, Response>
│       │   │   ├─ public class Validator  ← AbstractValidator<Command>
│       │   │   ├─ public record Command   ← IRequest<Response>
│       │   │   └─ public record Response
│       │   ├── CreateProductRequest.cs    ← DTO entrada
│       │   └── CreateProductValidator.cs  ← Reglas FluentValidation
│       │
│       ├── GetById/
│       │   └── GetProductById.cs
│       │       ├─ Query (lectura)
│       │       └─ Handler
│       │
│       ├── List/
│       │   └── ListProducts.cs
│       │       ├─ Query con paginación
│       │       └─ Handler
│       │
│       ├── Update/
│       │   └── UpdateProduct.cs
│       │
│       ├── Delete/
│       │   └── DeleteProduct.cs
│       │
│       ├── Domain/                        ← LÓGICA DE NEGOCIO
│       │   ├── Product.cs                 ← Entidad de dominio
│       │   ├── IProductRepository.cs      ← ISP (métodos específicos)
│       │   └── Exceptions/
│       │       ├── ProductNotFoundException.cs
│       │       ├── InsufficientStockException.cs
│       │       └── InvalidProductException.cs
│       │
│       ├── Infrastructure/                ← PERSISTENCIA
│       │   ├── ProductRepository.cs       ← Implementación EF Core
│       │   └── CatalogDbContext.cs        ← DbContext
│       │
│       └── Dtos/                          ← DATA TRANSFER OBJECTS
│           ├── ProductResponse.cs
│           ├── ProductListItemResponse.cs
│           └── CreateProductRequest.cs
│
├── Core/                                  ← COMPORTAMIENTOS TRANSVERSALES
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs          ← Pipeline MediatR
│   ├── Validators/
│   │   └── CustomValidators.cs            ← Extensiones reutilizables
│   ├── Errors/
│   │   └── ErrorTypes.cs                  ← Constantes de tipos error
│   └── Localization/
│       └── ValidationMessagesLocalizer.cs
│
├── Infrastructure/                        ← MIDDLEWARE Y SERVICIOS
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs   ← Manejo global de excepciones
│   │   └── ValidationExceptionHandler.cs  ← Manejo específico
│   ├── HealthChecks/
│   │   └── CustomHealthCheck.cs           ← IHealthCheck personalizado
│   ├── Mapping/
│   │   └── ProductMappingProfile.cs       ← AutoMapper (opcional)
│   └── Persistence/
│       └── Migrations/                    ← EF Core migrations
│
├── Program.cs                             ← CONFIGURACIÓN
│   ├─ DbContext setup
│   ├─ MediatR registration
│   ├─ FluentValidation registration
│   ├─ ValidationBehavior registration
│   ├─ Swagger setup
│   └─ HealthChecks setup
│
├── appsettings.json                       ← CONFIGURACIÓN
│   ├─ ConnectionStrings
│   └─ Logging
│
└── Catalog.API.csproj
    ├─ TargetFramework: net8.0
    ├─ PackageReferences:
    │   ├─ MediatR
    │   ├─ FluentValidation
    │   ├─ Swashbuckle.AspNetCore
    │   ├─ EntityFrameworkCore.PostgreSQL
    │   └─ AspNetCore.HealthChecks
    └─ GenerateDocumentationFile: true
```

---

## 🔗 Relación entre componentes

```
┌─────────────────────────────────────────────────────────────────────┐
│                      Endpoint (REST Controller)                      │
│                  Recibe HTTP Request, devuelve JSON                  │
└───────────┬─────────────────────────────────────────────────┬───────┘
            │                                                 │
            ▼                                                 ▼
    ┌──────────────┐                                 ┌───────────────┐
    │ DTO Request  │                                 │ DTO Response  │
    │ (entrada)    │                                 │ (salida)      │
    └──────┬───────┘                                 └───┬───────────┘
           │                                             │
           │ ToCommand()                                 │
           │                                             │
           ▼                                             │
    ┌──────────────┐                                     │
    │ MediatR      │                                     │
    │ .Send(Cmd)   │                                     │
    └──────┬───────┘                                     │
           │                                             │
           ▼                                             │
    ┌──────────────────┐                                │
    │ Validation       │  ◄─── Validadores (FluentVal) │
    │ Behavior         │       Reglas complejas         │
    │ (Pipeline)       │                                │
    └──────┬───────────┘                                │
           │                                             │
           ▼                                             │
    ┌──────────────┐                                     │
    │ Handler      │                                     │
    │ (Servicio)   │                                     │
    └──────┬───────┘                                     │
           │                                             │
           ▼                                             │
    ┌──────────────────────┐                            │
    │ IProductRepository   │ ◄─── ISP (Interface        │
    │ (Abstracción)        │       Segregation Principle)
    │ AddAsync()           │       Métodos específicos  │
    │ GetByIdAsync()       │       (no genéricos)       │
    │ UpdateAsync()        │                            │
    │ DeleteAsync()        │                            │
    └──────┬───────────────┘                            │
           │                                             │
           ▼                                             │
    ┌──────────────────────┐                            │
    │ ProductRepository    │                            │
    │ (Implementación)     │                            │
    └──────┬───────────────┘                            │
           │                                             │
           ▼                                             │
    ┌──────────────────────┐                            │
    │ EF Core + PostgreSQL │  ← INSERT/SELECT/UPDATE   │
    │ (Base de datos)      │                            │
    └──────────────────────┘                            │
           │                                             │
           │ Resultado                                  │
           │ (Product entity)                           │
           │                                             │
           ▼                                             │
    ┌──────────────────────┐                            │
    │ Handler transforma   │ ◄─── Mapping (opcional)    │
    │ Response             │       entity → DTO         │
    └──────────┬───────────┘                            │
               │                                         │
               └────────────────┬──────────────────────►┼──────────┐
                                │                        │          │
                                ▼                        │          │
                        ┌────────────────┐               │          │
                        │ Endpoint rec.  │               │          │
                        │ response       │               │          │
                        └────────┬───────┘               │          │
                                 │                       │          │
                     return CreatedAtAction()            │          │
                     HTTP 201 + Location header          │          │
                                 │                       │          │
                                 └───────────────────────┴──────────┘
                                           │
                                           ▼
                                   HTTP Response JSON
```

---

## 🎯 Matriz de responsabilidades (SOLID)

```
┌────────────────────┬───────────────┬─────────────────────────┐
│ Componente         │ Responsabilidad   │ Principio SOLID     │
├────────────────────┼───────────────┬─────────────────────────┤
│ Endpoint           │ HTTP parsing  │ SRP (Single Resp.)      │
│                    │ StatusCodes   │                         │
├────────────────────┼───────────────┼─────────────────────────┤
│ DTO                │ Mapeo datos   │ LSP (Liskov Subst.)     │
│                    │ Contract API  │                         │
├────────────────────┼───────────────┼─────────────────────────┤
│ Validator          │ Reglas entrada│ SRP                     │
│                    │ FluentVal     │ OCP (Open/Closed)       │
├────────────────────┼───────────────┼─────────────────────────┤
│ Handler            │ Lógica proceso│ SRP                     │
│                    │ Orquestar     │ DIP (Dependency Invert)│
├────────────────────┼───────────────┼─────────────────────────┤
│ Entity             │ Lógica negocio│ SRP                     │
│ (Product)          │ Invariantes   │ OCP                     │
├────────────────────┼───────────────┼─────────────────────────┤
│ Repository         │ Persistencia  │ ISP (Interface Segr.)   │
│ Interface          │ Métodos claros│ DIP                     │
├────────────────────┼───────────────┼─────────────────────────┤
│ Exceptions         │ Errores       │ SRP                     │
│                    │ Negocio       │ OCP                     │
├────────────────────┼───────────────┼─────────────────────────┤
│ ProblemDetails     │ Respuestas    │ LSP                     │
│                    │ Estándar      │                         │
├────────────────────┼───────────────┼─────────────────────────┤
│ Middleware         │ Atrapar errs  │ SRP                     │
│                    │ Transformar   │                         │
└────────────────────┴───────────────┴─────────────────────────┘

S = Single Responsibility    (Una responsabilidad por clase)
O = Open/Closed             (Abierto a extensión, cerrado a modificación)
L = Liskov Substitution      (Subclases reemplazan bases sin romper)
I = Interface Segregation    (Interfaces pequeñas y específicas)
D = Dependency Inversion     (Depender de abstracciones, no implementaciones)
```

---

## 📊 Comparativa: Sin Vertical Slice vs. Con Vertical Slice

```
┌─────────────────────────────────────────────────────────────┐
│ SIN VERTICAL SLICE (Capas tradicionales)                    │
├─────────────────────────────────────────────────────────────┤
│ Nueva feature "CreateProduct" requiere:                     │
│                                                             │
│ 1. Controllers/ProductController.cs         ← Crear/Editar │
│ 2. Services/ProductService.cs               ← Crear/Editar │
│ 3. Repositories/ProductRepository.cs        ← Crear/Editar │
│ 4. Models/Product.cs                        ← Crear/Editar │
│ 5. Models/Dtos/CreateProductRequest.cs      ← Crear/Editar │
│ 6. Models/Dtos/CreateProductResponse.cs     ← Crear/Editar │
│ 7. Validators/ProductValidator.cs           ← Crear/Editar │
│ 8. Exceptions/ProductExceptions.cs          ← Crear/Editar │
│                                                             │
│ TOTAL: 8 archivos en 8 carpetas diferentes                │
│        = Cambios dispersos                                 │
│        = Conflictos merge frecuentes                       │
│        = Difícil navegación                                │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ CON VERTICAL SLICE                                          │
├─────────────────────────────────────────────────────────────┤
│ Nueva feature "CreateProduct" requiere:                     │
│                                                             │
│ Features/Products/Create/CreateProduct.cs   ← TODO EN UNO  │
│  ├─ public class Endpoint                                   │
│  ├─ public class Handler                                    │
│  ├─ public class Validator                                  │
│  ├─ public record Command                                   │
│  ├─ public record Response                                  │
│  └─ public record CreateProductRequest                      │
│                                                             │
│ TOTAL: 1 archivo en 1 carpeta                              │
│        = Cambios localizados                                │
│        = CERO conflictos merge                              │
│        = Fácil navegación                                   │
│        = Máxima cohesión                                    │
└─────────────────────────────────────────────────────────────┘
```

---

**Fin del resumen visual - Módulo 3 completo ✅**
