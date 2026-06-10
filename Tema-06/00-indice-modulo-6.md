# Módulo 6 — Gestión de Datos en Microservicios

> **Duración estimada:** ~4 horas
> **Proyecto hilo conductor:** ShopMicro
> **Agregado de ejemplo:** `Pizza` (idéntico al curso de Spring Boot, trasladado a .NET / EF Core)
> **Persistencia:** EF Core **InMemory** (no entramos en PostgreSQL) · Basket en memoria (no entramos en Redis)

---

## Objetivo del módulo

Que el alumnado entienda **lo sencillo que es montar una capa de persistencia limpia en un
microservicio** apoyándose en el **Principio de Segregación de Interfaces (ISP)** de SOLID.

La idea central: una jerarquía de interfaces de *una sola operación* (`IAdd`, `IGet`, `IRemove`,
`IUpdate`) que cada repositorio concreto **compone** según lo que realmente necesita —ni más ni
menos—, con la implementación reutilizada vía **default interface methods** de C#. El dominio
permanece **puro** (sin anotaciones de EF) gracias a una clase de persistencia intermedia y un
mapper manual.

---

## Por qué InMemory y no PostgreSQL / Redis

El curso describe Catalog.API sobre PostgreSQL y Basket.API sobre Redis. En este módulo
**deliberadamente no entramos en ese mundo**: usamos `EF Core InMemory` y un almacén en memoria
para el carrito. Motivo pedagógico: queremos que el foco esté en el **diseño de la capa de datos**
(ISP, composición, dominio puro), no en aprovisionar infraestructura. El código que escribimos es
**idéntico** salvo el proveedor que se registra en el arranque; cambiar a PostgreSQL es una línea
de configuración.

---

## Lecciones

| # | Lección | Tipo |
|---|---|---|
| 6.1 | Database per service: el principio y sus consecuencias | Teoría |
| 6.2 | EF Core con InMemory en Catalog.API | Teoría + código |
| 6.3 | Repositorios con ISP: el núcleo reutilizable | ★ Núcleo |
| 6.4 | El agregado Pizza: persistencia, mapper y repositorio | ★ Núcleo |
| 6.5 | Redis como almacén del Basket.API (en memoria) | Código |
| 6.6 | El problema de la consistencia: transacciones distribuidas | Teoría |
| 6.7 | Patrón Saga (coreografía) — visión práctica | Teoría + esqueleto |
| 6.8 | Patrón Outbox para no perder eventos | Teoría + esqueleto |
| 6.9 | Resumen visual | Diagramas ASCII |

---

## Mapa conceptual del módulo

```
   6.1  ¿Por qué cada servicio su BD?           (el principio)
    │
    ▼
   6.2  EF Core InMemory                        (la herramienta)
    │
    ▼
   6.3  IAdd / IGet / IRemove / IUpdate + *Ef   (★ el núcleo ISP)
    │
    ▼
   6.4  Pizza: PizzaEf + Mapper + Repository    (★ el agregado)
    │
    ▼
   6.5  Basket en memoria                       (otro estilo de store)
    │
    ▼
   6.6  Consistencia distribuida                (el problema)
    │
    ├──▶ 6.7  Saga                              (la solución al flujo)
    └──▶ 6.8  Outbox                            (la solución a no perder eventos)
    │
    ▼
   6.9  Resumen visual
```

---

## Puntos clave del módulo

- **Cada microservicio es dueño de sus datos.** Nadie toca la BD de otro servicio directamente.
- **ISP no es teoría:** una interfaz por operación, compuesta a demanda, hace que un repositorio
  declare exactamente lo que sabe hacer.
- **El dominio no sabe que existe EF.** Clase de persistencia separada + mapper manual.
- **La consistencia fuerte distribuida no existe gratis:** se sustituye por consistencia
  eventual con Saga y se protege con Outbox.
