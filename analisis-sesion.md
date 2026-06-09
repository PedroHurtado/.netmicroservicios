# Análisis de sesión de Claude Code

> Material de aula — desglose de una sesión real de trabajo asistido por IA sobre un
> microservicio **.NET** con dominio **DDD**.

- **Fecha:** 9 de junio de 2026
- **Ventana horaria:** 08:10:04 → 08:24:34 UTC (~14 minutos)
- **Modelo:** `claude-opus-4-8`
- **Archivo de origen:** `session.jsonl` (66 registros, 15 turnos reales del asistente)

---

## Resumen de tokens

| Métrica | Valor |
|---|---:|
| Tokens de **salida** (generación real) | **14.293** |
| Tokens de **entrada** nuevos | 2.758 |
| Escritura de caché (*cache write*) | 21.572 |
| Lectura de caché (*cache read*) | 522.706 |

> **Cómo leer esto:** la *lectura de caché* (522 K) es el contexto que se **reutiliza y
> se vuelve a contar en cada turno**, no son tokens nuevos. El indicador de coste real es
> **salida (~14,3 K) + entrada nueva (~2,8 K)**. La caché abarata mucho las conversaciones
> largas porque el contexto repetido se cobra a tarifa reducida.

---

## Coste

| Concepto | Cálculo | Coste |
|----------|---------|-------|
| **Salida (output)** | 14.293 tokens × $15/MTok | **$0,214** |
| **Entrada nueva (input)** | 2.758 tokens × $3/MTok | **$0,008** |
| **Caché escrita (1ª vez)** | 21.572 tokens × $3/MTok | **$0,065** |
| **Caché leída** | 522.706 tokens × $0,30/MTok | **$0,157** |
| | | |
| **COSTE TOTAL** | | **~$0,44** |

---

## Tareas

### Tarea 1 — 08:10:04 ⚠️ *Interrumpida*

**Petición:** crear en `domain/core` la infraestructura para `EntiBase`, `AgrebarRoot`,
`Dominavent` (nombres con erratas en el prompt original).

- **Resultado:** cancelada por el usuario a los ~2 s (`[Request interrupted by user]`).
- **Trabajo generado:** ninguno (0 tokens de trabajo).
- **Lección:** interrumpir pronto cuando la petición no está bien formulada evita gastar
  tokens en una dirección equivocada.

---

### Tarea 2 — 08:16:52 → 08:22:18 (~5,5 min)

**Petición:** especificación detallada del núcleo de dominio:

- `EntityBase` abstracta, con **igualdad por `Id`** (dos entidades son iguales si comparten identificador).
- `AggregateRoot` que hereda de `EntityBase`, mantiene una lista de `DomainEvent` y expone `add` / `remove` / `clear`.
- `DomainEvent` con: `EventType`, `Aggregate`, `IdAggregate`, `User`, `TimeStamp`,
  `Estado` (pending | publish) y `PayLoad`.

**Qué hizo el asistente:**

- Exploró la estructura del proyecto (4× `Bash`).
- Creó 5 archivos (5× `Write`): `EntityBase.cs`, `AggregateRoot.cs`, `DomainEvent.cs`
  en `domain/core/`, más entidades en `domain/` (incluida `Pizza.cs`).

| Tokens | Valor |
|---|---:|
| Salida | **12.651** |
| Entrada | 2.748 |
| Cache write | 18.677 |
| Cache read | 315.181 |

---

### Tarea 3 — 08:23:24 → 08:24:34 (~1 min)

**Petición:** en `Pizza`, `Price` **no es persistible** → convertirlo en *getter*
calculado del dominio y eliminar el método privado `RecalculatePrice`.

**Qué hizo el asistente:**

- 4× `Edit` sobre `domain/Pizza.cs`: `Price` pasó a *getter* calculado y se eliminó el
  recálculo manual.

| Tokens | Valor |
|---|---:|
| Salida | **1.642** |
| Entrada | 10 |
| Cache write | 2.895 |
| Cache read | 207.525 |

---

## Distribución del trabajo

| Tarea | Salida | % del total | Herramientas |
|---|---:|---:|---|
| 1 — Infraestructura (interrumpida) | 0 | 0 % | — |
| 2 — Núcleo de dominio (DDD) | 12.651 | ~88 % | `Bash`×4, `Write`×5 |
| 3 — Ajuste de `Price` en `Pizza` | 1.642 | ~11 % | `Edit`×4 |

**Herramientas usadas en total:** `Bash`×4, `Write`×5, `Edit`×4.

---

## Conclusiones para el aula

1. **Un prompt vago se interrumpe y se reformula** (Tarea 1 → Tarea 2): merece la pena
   invertir en una buena especificación antes de dejar trabajar al modelo.
2. **La creación inicial concentra el coste** (~88 % de la generación); los ajustes
   posteriores son baratos.
3. **La caché domina el recuento bruto** pero no el coste real: en sesiones largas, el
   contexto reutilizado se cobra a tarifa reducida.
4. **Crear ≠ editar:** la herramienta `Write` aparece al construir desde cero; `Edit` al
   refinar (igualdad por `Id`, `Price` como propiedad calculada).
