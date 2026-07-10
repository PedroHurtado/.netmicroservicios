# El Dispatcher Explícito de Fudie

> **Contexto:** alternativa a MediatR diseñada para Fudie.
> **Requisitos:** .NET 7+ (usa *static abstract/virtual members in interfaces*, C# 11). Verificado sobre .NET 8.
> **Origen:** discrepancia con los `IPipelineBehavior` de MediatR — configuración global e implícita para una decisión que debería ser declarativa y local.

---

## 1. El problema que resuelve

Con MediatR, saber qué cross-cutting (logging, validación, transacción) se aplica a un request exige deducirlo del registro del DI y de las restricciones genéricas repartidas por los behaviors. Nada en el mensaje ni en el endpoint lo hace visible. Consecuencias:

- **Transacciones indiscriminadas:** un behavior global envuelve también las queries en transacciones que no necesitan.
- **Errores silenciosos:** si la configuración global está mal, el comando corre sin validación o sin transacción y nada chilla.
- **Conocimiento no localizable:** "¿este comando lleva transacción?" no se responde con un F12.

El punto de comparación es `@Transactional` de Spring: la propiedad **vive en la operación**, visible, local, deliberada.

**Síntesis:** lo que es *invariante del mensaje* (transaccionalidad, validación) debe declararse **en el tipo** y verificarlo **el compilador**; lo que es *variable de la invocación* (timeout puntual, cancelación) se elige en el call site.

---

## 2. El modelo: tres niveles

```
Nivel 1 — CATEGORÍA      ICommand / IQuery (definidos por CADA microservicio)
                         declaran el pipeline por defecto de su categoría.
                         Declarar ": ICommand" ES declarar la invariante (≈ @Transactional).

Nivel 2 — TIPO           Un comando concreto sobreescribe Pipeline cuando su
                         invariante difiere. La desviación queda firmada y visible
                         en el propio tipo.

Nivel 3 — INVOCACIÓN     .WithTimeout(...) / .WithCancellation(...) para lo
                         variable de ESTA llamada. No puede quitar etapas.
```

Cada nivel responde una pregunta distinta: *qué es* (categoría), *qué es este en particular* (tipo), *qué pasa esta vez* (invocación). Quien lee el código sabe dónde mirar.

---

## 3. Arquitectura: mecanismo vs. política

La decisión estructural clave: **el Core es solo mecanismo; la política la define cada microservicio.**

```
┌─────────────────────────── Fudie.Core (CERRADO) ───────────────────────────┐
│                                                                            │
│  IDispatchable ── el contrato: sin Pipeline declarado, Send no compila     │
│  Pipeline      ── lista abierta e inmutable: .Use(...) admite N etapas     │
│  IStage        ── el punto de extensión                                    │
│  Sender        ── resuelve TMessage.Pipeline (estático) + handler (DI)     │
│                                                                            │
│  + Stages/ ── CATÁLOGO OPCIONAL: Logging, Validation, Transaction          │
│              (con sus puertos: IMessageValidator, ITransactionManager)     │
└────────────────────────────────────────────────────────────────────────────┘
                                     ▲
                                     │ referencia
┌─────────────────────── Cada microservicio (POLÍTICA) ──────────────────────┐
│                                                                            │
│  Sus marcadores ICommand / IQuery (≈25 líneas, una vez):                   │
│    · qué etapas componen su default y en qué orden                         │
│    · con o sin validación; FluentValidation, DataAnnotations o nada        │
│  Sus etapas propias: Audit, Retry, Idempotency, Caching... (clases IStage) │
│  Sus slices: Command/Query + Handler + Validator (nested classes)          │
└────────────────────────────────────────────────────────────────────────────┘
```

En el Core **no hay** Logging, ni Validation, ni Transaction obligatorios, ni orden fijo, ni FluentValidation, ni límite de etapas. Las tres etapas incluidas son catálogo, no camino obligado.

---

## 4. El contrato del Core

```csharp
// Lo ÚNICO que el Core exige a un mensaje:
public interface IDispatchable<TSelf, TResponse>
    where TSelf : IDispatchable<TSelf, TResponse>
{
    static abstract Pipeline<TSelf, TResponse> Pipeline { get; }
}
```

El `static abstract` es la pieza que elimina el olvido: **no existe el "Send pelado"**. Un mensaje que no declare (directa o indirectamente) su pipeline no compila al pasar por `Send`.

El pipeline es una **descripción inmutable** (lista ordenada de fábricas de etapas) sin servicios dentro — por eso puede vivir en una propiedad `static` del tipo. El orden de la lista es el orden de **anidamiento**: la primera etapa es la más externa, la última la más pegada al handler.

```csharp
public interface IStage<TMessage, TResponse>
{
    Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken ct);
}
```

---

## 5. La política del microservicio

Cada servicio define sus marcadores implementando el contrato del Core (DIM estático, C# 11 — verificado):

```csharp
// ARCHIVO DEL MICROSERVICIO, no del Core
public interface ICommand<TSelf, TResponse>
    : IDispatchable<TSelf, TResponse>, IValidatable, ITransactional
    where TSelf : ICommand<TSelf, TResponse>
{
    static Pipeline<TSelf, TResponse> IDispatchable<TSelf, TResponse>.Pipeline =>
        Pipelines.For<TSelf, TResponse>()
            .Use<LoggingStage<TSelf, TResponse>>()
            .Use(static sp => new ValidationStage<TSelf, TResponse>(
                sp.GetServices<IMessageValidator<TSelf>>()))
            .Use<TransactionStage<TSelf, TResponse>>();
}

public interface IQuery<TSelf, TResponse> : IDispatchable<TSelf, TResponse>
    where TSelf : IQuery<TSelf, TResponse>
{
    static Pipeline<TSelf, TResponse> IDispatchable<TSelf, TResponse>.Pipeline =>
        Pipelines.For<TSelf, TResponse>()
            .Use<LoggingStage<TSelf, TResponse>>();
}
```

Aquí decide cada servicio: sin validación → quita la línea; otra tecnología → otra etapa u otro adaptador del puerto; ocho etapas → ocho `Use`. **El Core no se entera.**

### El día a día del programador (idéntico a MediatR)

```csharp
public static class CreateProduct
{
    public sealed record Command(string Name, decimal Price) : ICommand<Command, Guid>;

    public sealed class Validator : AbstractValidator<Command> { /* FluentValidation normal */ }

    public sealed class Handler : IHandler<Command, Guid>
    {
        public Task<Guid> Handle(Command message, CancellationToken ct) { /* ... */ }
    }
}

// Uso:
var id = await sender.Send(new CreateProduct.Command("Mesa", 149.90m));
```

Sin escribir nada más, ese comando lleva el pipeline completo de su categoría.

### Override en el tipo (nivel 2) y etapas propias

```csharp
public sealed record Command(int Items) : ICommand<Command, int>
{
    // Desviación del default: firmada y visible en el tipo. Cinco etapas,
    // dos de ellas propias del servicio (Audit y Retry).
    public static Pipeline<Command, int> Pipeline =>
        Pipelines.For<Command, int>()
            .Use<LoggingStage<Command, int>>()
            .Use<AuditStage<Command, int>>()                      // propia, vía DI
            .Use(static sp => new ValidationStage<Command, int>(
                sp.GetServices<IMessageValidator<Command>>()))
            .Use(static _ => new RetryStage<Command, int>(3))     // propia, parametrizada
            .Use<TransactionStage<Command, int>>();
}
```

Una etapa nueva es una clase `IStage` en el proyecto del servicio. Nota sobre el orden: como composición = anidamiento, `Retry` antes de `Transaction` significa que **cada reintento abre su propia transacción** (verificado en la demo: BEGIN→ROLLBACK→retry→BEGIN→COMMIT).

### Variables de invocación (nivel 3)

```csharp
var result = await sender.Send(command)
                         .WithTimeout(TimeSpan.FromSeconds(5))
                         .WithCancellation(ct);
```

Solo modula; no existe API para quitar etapas desde el call site. *Matiz:* la ejecución arranca en el `await` — un `Send` nunca esperado nunca ejecuta (inocuo en comandos con respuesta; si hubiera comandos `void`, conviene un analizador).

---

## 6. Garantías del compilador (todas verificadas)

| # | Garantía | Mecanismo | Error si se viola |
|---|---|---|---|
| 1 | Ningún mensaje se envía sin pipeline declarado | `static abstract Pipeline` en el contrato | No compila el `Send` |
| 2 | Una query no puede llevar transacción | `TransactionStage where TMessage : ITransactional`; `IQuery` no implementa `ITransactional` | CS0311 |
| 3 | Resolución del pipeline sin DI ni reflexión | Despacho estático: override del tipo o default del marcador | — (es el funcionamiento) |
| 4 | Una etapa puede exigir capacidades al mensaje | Constraint genérico **en la etapa**, contra marcadores de capacidad | CS0311 en el punto de composición |

La garantía 4 es la que mantiene la seguridad en un modelo abierto: la restricción ya no vive en un builder cerrado de 3 etapas, sino que **viaja con cada etapa** — también con las tuyas.

---

## 7. Cómo se ejecuta un Send (traza)

```
await sender.Send(new CreateProduct.Command("Mesa", 149.90m)).WithTimeout(5s)

 1. COMPILACIÓN   Command : ICommand → encaja en Send(IDispatchable<,>);
                  se infieren TMessage y TResponse de la interfaz implementada.
 2. COMPILACIÓN   TMessage.Pipeline → despacho estático: override del tipo
                  si existe; si no, default del marcador del servicio.
                  Cero reflexión, cero búsqueda en el contenedor.
 3. DIFERIMIENTO  Send devuelve PendingDispatch (struct). WithTimeout acumula
                  opciones. La ejecución arranca en el await.
 4. MATERIALIZAR  Se resuelve IHandler<Command, Guid> del DI (scope actual)
                  y las etapas se construyen con el IServiceProvider.
 5. CADENA        De dentro hacia fuera:  Logging( Validation( Transaction( Handler )))
 6. EJECUCIÓN     Logging mide todo → Validation corta SIN llamar a next()
                  si hay errores (la transacción nunca se abre para input
                  basura) → Transaction delega en ITransactionManager →
                  Handler.
```

**Encaje con el Outbox de ShopMicro/Fudie:** dentro del handler, `SaveChanges` despacha los DomainEvents (efectos locales + filas de Outbox) antes de su commit. El `BeginTransaction` de la etapa solo aporta cuando el comando hace más de un `SaveChanges`; con uno solo, EF ya es atómico (la etapa es redundante pero inocua — y las queries, donde sería absurda, no pueden tenerla).

---

## 8. Comparativa con MediatR

### Tabla resumen

| Aspecto | MediatR + behaviors | Dispatcher Fudie |
|---|---|---|
| Dónde se declara el cross-cutting | Registro global en el DI + constraints en los behaviors | En el tipo del mensaje (marcador o override) |
| ¿Visible con F12 desde el comando? | No (hay que reconstruirlo del `Program.cs`) | Sí (marcador → default; override → en el tipo) |
| Comando sin validación/transacción por error | Corre en silencio | El olvido estructural no existe: sin pipeline no compila |
| Transacción en queries | Posible si el behavior no discrimina (mitigable con marcadores `ICommand`/`IQuery` + `where`) | Imposible: CS0311 |
| Resolución de pipeline | Runtime: reflexión + contenedor | Compilación: despacho estático |
| Overrides por mensaje concreto | Difícil (más constraints, más interfaces, más magia) | Natural: propiedad `Pipeline` en el tipo |
| Variables por invocación (timeout puntual) | No contemplado | `PendingDispatch.WithTimeout/WithCancellation` |
| Extensibilidad de etapas | Buena: un `IPipelineBehavior` + registro global | Buena: una clase `IStage` + `Use(...)` donde toque |
| Publicación de eventos (`Publish`/`INotification`) | Incluido | **No incluido** (pieza pendiente o se conserva MediatR para eventos) |
| Streams (`IStreamPipelineBehavior`) | Incluido | No incluido |
| Código a mantener | Cero (dependencia) | ~300 líneas propias en el Core |
| Curva de entrada del equipo | Baja: patrón ultraconocido, documentación masiva | Media: *static abstracts* y DIM estático son C# moderno poco extendido |
| Versión mínima | .NET Standard (cualquier proyecto vivo) | .NET 7+ / C# 11 |
| Licencia | Comercial desde 2025 (gratuita solo bajo ciertos umbrales) | Propia, sin dependencia de terceros |

### Pros del dispatcher Fudie

1. **El olvido estructural no existe.** La crítica de origen queda resuelta de raíz: la invariante vive en el tipo y la verifica el compilador, no la disciplina ni el code review.
2. **Conocimiento localizable.** "¿Qué le pasa a este comando?" se responde leyendo el comando o su marcador. Una sola fuente de verdad por servicio.
3. **Categorías con garantías.** Queries sin transacción —y cualquier otra regla de capacidad que definas— por construcción, no por convención.
4. **Política por microservicio.** Cada servicio decide etapas, orden y tecnología de validación sin tocar el Core ni heredar decisiones ajenas.
5. **Tres niveles bien separados.** Invariante de categoría / invariante del tipo / variable de la invocación: cada cosa en su sitio, sin mezclar.
6. **Cero magia en runtime.** Sin reflexión para resolver pipelines, sin escaneo de behaviors, sin sorpresas de orden de registro.
7. **Sin dependencia comercial.** Tras el cambio de licencia de MediatR, el coste de la alternativa propia se compara con un coste de licencia, no con "gratis".

### Contras del dispatcher Fudie (honestos)

1. **Es código tuyo.** ~300 líneas que mantener, testear y documentar. MediatR es de Jimmy Bogard; esto es de Pedro. Cada bug es tuyo.
2. **C# avanzado.** *Static abstract members*, DIM estático, genéricos auto-referenciados (`TSelf`). Un equipo junior tardará en sentirse cómodo modificando los marcadores (usarlos, en cambio, es trivial).
3. **No cubre eventos.** El `Publish`/`INotification` que hoy usa el despacho de DomainEvents en `SaveChanges` no está. Opciones: (a) mantener MediatR solo para eventos, (b) escribir un publicador propio (pieza pequeña, sin pipelines: foreach handler). Decisión pendiente.
4. **Ergonomía del genérico doble.** Los marcadores exigen `ICommand<Command, Guid>` con el propio tipo como primer parámetro (patrón CRTP). Funciona y el IDE ayuda, pero es más ruido que `IRequest<Guid>`.
5. **Ejecución diferida.** `PendingDispatch` ejecuta al `await`; un `Send` olvidado sin await no hace nada. Mitigable con un analizador, pero es una arista que MediatR no tiene.
6. **Ecosistema cero.** Sin extensiones de terceros, sin respuestas en Stack Overflow, sin experiencia previa de los nuevos fichajes.

### Cuándo cada uno

- **MediatR (+ marcadores `ICommand`/`IQuery` y `where` en los behaviors):** equipos que ya lo dominan, proyectos donde el coste de mantener infraestructura propia no se justifica, y contextos docentes (el alumno de 5 horas no apreciaría más). Es la solución del curso.
- **Dispatcher Fudie:** producto propio a largo plazo donde se quiere que las invariantes sean verificadas por el compilador, política por microservicio y cero dependencia de licencia. Es la solución de Fudie.

---

## Puntos clave

- La transaccionalidad y la validación son **invariantes del mensaje**: se declaran en el tipo y las verifica el compilador. El timeout o la cancelación son **variables de la invocación**: se eligen en el call site.
- **Core = mecanismo** (contrato + pipeline abierto + etapa + sender, cerrado), **microservicio = política** (sus marcadores, sus etapas, su orden, su tecnología de validación).
- El builder es abierto (`Use`, N etapas); la seguridad no la da un builder cerrado sino los **constraints que viajan en cada etapa** contra marcadores de capacidad.
- El orden de composición es el orden de **anidamiento**: lo primero es lo más externo. `Retry` antes de `Transaction` ⇒ cada reintento con transacción nueva.
- El día a día del programador es idéntico a MediatR: record + validator + handler + `Send`. Lo que cambia son las **garantías**, no la ergonomía.
- Pieza pendiente para sustituir del todo a MediatR en Fudie: el publicador de eventos del `SaveChanges`.
