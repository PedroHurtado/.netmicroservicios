using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.Core.Dispatch;

// ═════════════════════════════════════════════════════════════════════════════
//  EL CORE ES SOLO MECANISMO. Cuatro piezas, cerradas. Aquí NO hay:
//  ni Logging, ni Validation, ni Transaction, ni FluentValidation, ni orden,
//  ni número de etapas. Todo eso es POLÍTICA y la define cada microservicio.
// ═════════════════════════════════════════════════════════════════════════════

// ── Pieza 1: el contrato ─────────────────────────────────────────────────────
/// <summary>
/// Lo único que el Core exige: un mensaje despachable declara su Pipeline.
/// Sin Pipeline declarado, Send no compila.
/// </summary>
public interface IDispatchable<TSelf, TResponse>
    where TSelf : IDispatchable<TSelf, TResponse>
{
    static abstract Pipeline<TSelf, TResponse> Pipeline { get; }
}

/// <summary>Handler de un mensaje. Uno por mensaje, resuelto del contenedor.</summary>
public interface IHandler<in TMessage, TResponse>
{
    Task<TResponse> Handle(TMessage message, CancellationToken cancellationToken);
}

// ── Pieza 2: la etapa (el punto de extensión) ────────────────────────────────
public delegate Task<TResponse> Next<TResponse>();

/// <summary>
/// Una etapa envuelve a next. Implementa tantas como necesites (4, 5, 8...):
/// el Core no las conoce ni las limita. Si tu etapa requiere una capacidad del
/// mensaje, decláralo con un constraint genérico en TU etapa: el compilador lo
/// verificará en el punto donde se componga el pipeline.
/// </summary>
public interface IStage<TMessage, TResponse>
{
    Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken cancellationToken);
}

// ── Pieza 3: el pipeline (lista abierta, inmutable, sin servicios dentro) ────
/// <summary>
/// Descripción inmutable: lista ordenada de fábricas de etapas. El orden de la
/// lista es el orden de ANIDAMIENTO (primera = más externa). Es una descripción
/// sin servicios, por eso puede vivir en una propiedad static del tipo; las
/// etapas se materializan con el IServiceProvider al ejecutar.
/// </summary>
public sealed class Pipeline<TMessage, TResponse>
{
    private readonly ImmutableArray<Func<IServiceProvider, IStage<TMessage, TResponse>>> _stages;

    internal static readonly Pipeline<TMessage, TResponse> Empty = new([]);

    private Pipeline(ImmutableArray<Func<IServiceProvider, IStage<TMessage, TResponse>>> stages)
        => _stages = stages;

    /// <summary>Añade una etapa cuyas dependencias resuelve el contenedor.</summary>
    public Pipeline<TMessage, TResponse> Use<TStage>()
        where TStage : IStage<TMessage, TResponse> =>
        new(_stages.Add(static sp => ActivatorUtilities.CreateInstance<TStage>(sp)));

    /// <summary>Añade una etapa construida a mano (etapas parametrizadas: Retry(3), Timeout(5s)...).</summary>
    public Pipeline<TMessage, TResponse> Use(Func<IServiceProvider, IStage<TMessage, TResponse>> factory) =>
        new(_stages.Add(factory));

    internal Task<TResponse> ExecuteAsync(
        TMessage message,
        IServiceProvider services,
        IHandler<TMessage, TResponse> handler,
        CancellationToken cancellationToken)
    {
        Next<TResponse> next = () => handler.Handle(message, cancellationToken);

        for (var i = _stages.Length - 1; i >= 0; i--)
        {
            var stage = _stages[i](services);
            var inner = next;
            next = () => stage.Invoke(message, inner, cancellationToken);
        }

        return next();
    }
}

/// <summary>Punto de partida de la composición.</summary>
public static class Pipelines
{
    public static Pipeline<TMessage, TResponse> For<TMessage, TResponse>() =>
        Pipeline<TMessage, TResponse>.Empty;
}

// ── Pieza 4: el Sender ───────────────────────────────────────────────────────
public interface ISender
{
    PendingDispatch<TResponse> Send<TMessage, TResponse>(IDispatchable<TMessage, TResponse> message)
        where TMessage : IDispatchable<TMessage, TResponse>;
}

public sealed class Sender(IServiceProvider services) : ISender
{
    public PendingDispatch<TResponse> Send<TMessage, TResponse>(IDispatchable<TMessage, TResponse> message)
        where TMessage : IDispatchable<TMessage, TResponse>
    {
        var typed = (TMessage)message;
        var pipeline = TMessage.Pipeline;   // resolución ESTÁTICA: override del tipo,
                                            // o default del marcador del microservicio
        return new PendingDispatch<TResponse>(options => ExecuteAsync(typed, pipeline, options));
    }

    private async Task<TResponse> ExecuteAsync<TMessage, TResponse>(
        TMessage message, Pipeline<TMessage, TResponse> pipeline, InvocationOptions options)
    {
        CancellationTokenSource? timeoutCts = null;
        try
        {
            var cancellationToken = options.CancellationToken;
            if (options.Timeout is { } timeout)
            {
                timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);
                cancellationToken = timeoutCts.Token;
            }

            var handler = services.GetRequiredService<IHandler<TMessage, TResponse>>();
            return await pipeline.ExecuteAsync(message, services, handler, cancellationToken);
        }
        finally
        {
            timeoutCts?.Dispose();
        }
    }
}

/// <summary>
/// Variables de la invocación: solo modula (timeout, cancelación). No expone
/// forma de quitar etapas. Ejecuta al hacer await (o AsTask).
/// </summary>
public readonly struct PendingDispatch<TResponse>
{
    private readonly Func<InvocationOptions, Task<TResponse>> _execute;
    private readonly InvocationOptions _options;

    internal PendingDispatch(Func<InvocationOptions, Task<TResponse>> execute)
        : this(execute, InvocationOptions.None) { }

    private PendingDispatch(Func<InvocationOptions, Task<TResponse>> execute, InvocationOptions options)
    {
        _execute = execute;
        _options = options;
    }

    public PendingDispatch<TResponse> WithTimeout(TimeSpan timeout) =>
        new(_execute, _options with { Timeout = timeout });

    public PendingDispatch<TResponse> WithCancellation(CancellationToken cancellationToken) =>
        new(_execute, _options with { CancellationToken = cancellationToken });

    public Task<TResponse> AsTask() => _execute(_options);

    public TaskAwaiter<TResponse> GetAwaiter() => AsTask().GetAwaiter();
}

public sealed record InvocationOptions
{
    public static readonly InvocationOptions None = new();

    public TimeSpan? Timeout { get; init; }
    public CancellationToken CancellationToken { get; init; }
}
