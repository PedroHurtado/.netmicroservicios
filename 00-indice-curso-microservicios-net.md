# Microservicios con .NET — Índice del Curso

> **Duración:** 25 horas (≈26h de contenido para dar margen)
> **Versión base:** .NET 8 (LTS) · soporte hasta noviembre de 2026
> **Lenguaje:** C#
> **Modalidad de ejemplos:** snippets concretos por concepto + proyecto hilo conductor que evoluciona durante todo el curso

---

## Objetivos del curso

- Comprender los conceptos fundamentales de la arquitectura de microservicios y sus ventajas.
- Aprender a desarrollar microservicios utilizando ASP.NET Core.
- Aprender la contenerización de microservicios con Docker y su orquestación mediante Docker Compose y Kubernetes.
- Implementar comunicación resiliente entre servicios (HTTP, Refit, gRPC y mensajería asíncrona).
- Asegurar microservicios con JWT y OAuth2 / OpenID Connect.
- Gestionar datos con el patrón *database per service* y resolver la consistencia distribuida.
- Instrumentar el sistema con observabilidad (logs, métricas y trazas distribuidas).

## Requisitos previos

- Conocimientos de .NET y C#.
- Fundamentos de desarrollo web.
- Conceptos básicos de bases de datos.
- Familiaridad con la línea de comandos.

---

## Nota sobre la versión de .NET

El curso usa **.NET 8 (LTS)** como base. Aunque .NET 10 (LTS) ya está disponible, .NET 8 sigue siendo
la opción más extendida en empresa y con el ecosistema de microservicios más maduro y documentado.
Su soporte finaliza en **noviembre de 2026**, por lo que el módulo final incluye la **ruta de migración a .NET 10**.
El código del curso es prácticamente idéntico en ambas versiones.

---

## Proyecto hilo conductor: "ShopMicro"

Un e-commerce mínimo descompuesto en servicios que se construye módulo a módulo:

| Servicio | Responsabilidad | Tecnología destacada |
|---|---|---|
| **Catalog.API** | Catálogo de productos | EF Core + PostgreSQL |
| **Basket.API** | Carrito de la compra | Redis |
| **Ordering.API** | Gestión de pedidos | EF Core + mensajería |
| **Identity** | Autenticación y emisión de tokens | JWT / OpenID Connect |
| **Gateway** | Punto de entrada único | YARP |

---

## Módulo 1 — Introducción a los Microservicios *(~3h)*

- 1.1 ¿Qué es un microservicio? Definición y características
- 1.2 Monolito vs. microservicios: cuándo SÍ y cuándo NO
- 1.3 Ventajas, costes y falacias de los sistemas distribuidos
- 1.4 Conceptos clave: *bounded context*, acoplamiento, cohesión
- 1.5 Anatomía de ShopMicro (visión global del proyecto del curso)

## Módulo 2 — Fundamentos de .NET para Microservicios *(~3h)*

- 2.1 El SDK de .NET 8 y la CLI (`dotnet new`, `build`, `run`)
- 2.2 Anatomía de un proyecto ASP.NET Core Minimal API
- 2.3 Inyección de dependencias y el contenedor de servicios
- 2.4 Configuración (`appsettings`, entornos, variables de entorno, `IOptions`)
- 2.5 Logging estructurado y *middleware pipeline*

## Módulo 3 — Desarrollo de Microservicios con ASP.NET Core *(~5h)*

- 3.1 Diseño de una API REST: recursos, verbos, códigos de estado
- 3.2 Construcción de **Catalog.API** desde cero (Minimal API)
- 3.3 Capas, DTOs y mapeo
- 3.4 Validación de entrada (FluentValidation)
- 3.5 Manejo global de errores (`ProblemDetails`)
- 3.6 Documentación con OpenAPI / Swagger
- 3.7 *Health checks*

## Módulo 4 — Comunicación entre Microservicios *(~5h)*

- 4.1 Síncrona vs. asíncrona: el gran dilema
- 4.2 Comunicación HTTP con `HttpClientFactory`: la forma base
- 4.3 Refit: clientes HTTP declarativos por interface (*typed clients* sin *boilerplate*)
- 4.4 Resiliencia con Polly: *retry*, *timeout*, *circuit breaker*
- 4.5 gRPC: contratos con Protobuf y comunicación de alto rendimiento (REST vs. gRPC, cuándo cada uno)
- 4.6 Mensajería asíncrona: conceptos (*broker*, cola, *topic*, eventos)
- 4.7 Implementación con RabbitMQ + MassTransit
- 4.8 Patrón: publicar un evento `OrderCreated` y consumirlo
- 4.9 API Gateway con YARP

## Módulo 5 — Seguridad en Microservicios *(~3h)*

- 5.1 Autenticación vs. autorización en sistemas distribuidos
- 5.2 JWT: estructura y validación
- 5.3 OAuth2 / OpenID Connect (panorama)
- 5.4 Proteger Catalog.API y Ordering.API con JWT Bearer
- 5.5 Autorización por roles y políticas
- 5.6 Secretos y configuración sensible (*user-secrets*, variables de entorno)

## Módulo 6 — Gestión de Datos en Microservicios *(~4h)*

- 6.1 *Database per service*: el principio y sus consecuencias
- 6.2 EF Core con PostgreSQL en Catalog.API (migraciones incluidas)
- 6.3 Redis como almacén del Basket.API
- 6.4 El problema de la consistencia: transacciones distribuidas
- 6.5 Patrón Saga (coreografía) — visión práctica
- 6.6 Patrón Outbox para no perder eventos

## Módulo 7 — Implementación, Despliegue y Observabilidad *(~4h)*

- 7.1 Contenerización: Dockerfile *multi-stage* para un servicio .NET
- 7.2 Orquestación local con Docker Compose (toda la solución en marcha)
- 7.3 Los tres pilares de la observabilidad: logs, métricas y trazas
- 7.4 Trazas distribuidas con OpenTelemetry (instrumentación, propagación de contexto, correlación entre servicios)
- 7.5 Visualización con Jaeger (seguir una petición a través de ShopMicro)
- 7.6 Introducción a Kubernetes: *pods*, *deployments*, *services*
- 7.7 Desplegar ShopMicro en Kubernetes (manifiestos YAML)
- 7.8 Configuración, *secrets* y escalado en K8s
- 7.9 Cierre: *checklist* de producción y ruta de migración a .NET 10

---

## Distribución orientativa de horas

| Módulo | Tema | Horas |
|---|---|---|
| 1 | Introducción a los Microservicios | 3 |
| 2 | Fundamentos de .NET | 3 |
| 3 | Desarrollo con ASP.NET Core | 5 |
| 4 | Comunicación entre Microservicios | 5 |
| 5 | Seguridad | 3 |
| 6 | Gestión de Datos | 4 |
| 7 | Implementación, Despliegue y Observabilidad | 4 |
| | **Total** | **~26h** |
