using Fudie.Core.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Fudie.Core.Dispatch.Stages;

namespace ShopMicro.Catalog.Application;

// ═════════════════════════════════════════════════════════════════════════════
//  ESTE ARCHIVO ES DEL MICROSERVICIO, NO DEL CORE. Es su política: qué es un
//  comando y una query AQUÍ. Otro microservicio escribe el suyo distinto
//  (sin validación, con otra tecnología, con 8 etapas...) sin tocar el Core.
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Comando de ESTE servicio: Logging → Validation → Transaction.
/// El orden y las etapas se deciden aquí, una vez, a la vista.
/// </summary>
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

/// <summary>
/// Query de ESTE servicio: solo Logging. No implementa ITransactional:
/// componer TransactionStage sobre una query NO COMPILA.
/// </summary>
public interface IQuery<TSelf, TResponse>
    : IDispatchable<TSelf, TResponse>
    where TSelf : IQuery<TSelf, TResponse>
{
    static Pipeline<TSelf, TResponse> IDispatchable<TSelf, TResponse>.Pipeline =>
        Pipelines.For<TSelf, TResponse>()
            .Use<LoggingStage<TSelf, TResponse>>();
}

// ═════ ETAPAS PROPIAS DEL SERVICIO (la 4ª y la 5ª que pedías) ═════
// Una etapa nueva = una clase en TU proyecto. El Core no se entera.

/// <summary>4ª etapa: auditoría. Sin parámetros → se compone con Use&lt;T&gt;().</summary>
public sealed class AuditStage<TMessage, TResponse> : IStage<TMessage, TResponse>
{
    public async Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"   [AUDIT] {typeof(TMessage).Name} :: {message}");
        return await next();
    }
}

/// <summary>5ª etapa: reintentos. Parametrizada → se compone con Use(sp => new ...).</summary>
public sealed class RetryStage<TMessage, TResponse>(int attempts) : IStage<TMessage, TResponse>
{
    public async Task<TResponse> Invoke(TMessage message, Next<TResponse> next, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await next();
            }
            catch (Exception) when (attempt < attempts)
            {
                Console.WriteLine($"   [RETRY] intento {attempt} fallido, reintentando...");
            }
        }
    }
}
