using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Fudie.Core.Dispatch.Stages;

// ═════════════════════════════════════════════════════════════════════════════
//  ETAPAS DE SERIE — OPCIONALES. El Core no las usa: están aquí como catálogo.
//  Cada microservicio decide cuáles componer, en qué orden, o ninguna.
//  Una etapa propia es lo mismo que estas: una clase IStage en TU proyecto.
// ═════════════════════════════════════════════════════════════════════════════

// ── Marcadores de CAPACIDAD ──────────────────────────────────────────────────
// Así viaja la garantía sin cerrar el builder: la restricción va EN la etapa,
// contra un marcador de capacidad. El compilador la verifica donde se compone.

/// <summary>El mensaje admite ejecutarse dentro de una transacción.</summary>
public interface ITransactional;

/// <summary>El mensaje admite validación de entrada.</summary>
public interface IValidatable;

// ── Logging ──────────────────────────────────────────────────────────────────
public sealed class LoggingStage<TMessage, TResponse>(ILoggerFactory loggerFactory)
    : IStage<TMessage, TResponse>
{
    public async Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(TMessage).FullName ?? typeof(TMessage).Name);
        var name = typeof(TMessage).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Despachando {Message}", name);
        try
        {
            var response = await next();
            logger.LogInformation("{Message} completado en {Elapsed} ms", name, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "{Message} falló tras {Elapsed} ms", name, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

// ── Validación (puerto propio; FluentValidation es UN adaptador posible) ─────
public interface IMessageValidator<in TMessage>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TMessage message, CancellationToken cancellationToken);
}

public sealed record ValidationError(string PropertyName, string ErrorMessage);

public sealed class MessageValidationException(string messageType, IReadOnlyList<ValidationError> errors)
    : Exception($"Validación fallida para '{messageType}' ({errors.Count} error(es)).")
{
    public string MessageType { get; } = messageType;
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}

public sealed class ValidationStage<TMessage, TResponse>(IEnumerable<IMessageValidator<TMessage>> validators)
    : IStage<TMessage, TResponse>
    where TMessage : IValidatable
{
    public async Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken cancellationToken)
    {
        List<ValidationError>? errors = null;

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            if (result.Count > 0)
                (errors ??= []).AddRange(result);
        }

        if (errors is { Count: > 0 })
            throw new MessageValidationException(typeof(TMessage).Name, errors);

        return await next();
    }
}

// ── Transacción (puerto propio; la implementación EF vive en Infrastructure) ─
public interface ITransactionManager
{
    Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken);
}

public sealed class TransactionStage<TMessage, TResponse>(ITransactionManager transactions)
    : IStage<TMessage, TResponse>
    where TMessage : ITransactional   // ← la garantía: un tipo sin esta capacidad
                                      //   no puede componer esta etapa (no compila)
{
    public Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken cancellationToken) =>
        transactions.ExecuteAsync(_ => next(), cancellationToken);
}
