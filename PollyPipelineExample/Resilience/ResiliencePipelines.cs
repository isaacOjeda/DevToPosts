using Polly;
using Polly.Retry;

namespace PollyPipelineExample.Resilience;

/// <summary>
/// Factory para crear pipelines de resiliencia reutilizables.
/// Centraliza la configuración de estrategias de resiliencia.
/// </summary>
public static class ResiliencePipelines
{
    /// <summary>
    /// Pipeline básico de reintentos con backoff exponencial.
    /// Ideal para operaciones que pueden fallar temporalmente.
    /// </summary>
    public static ResiliencePipeline CreateRetryPipeline(int maxRetries = 3)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true, // Añade variación aleatoria para evitar thundering herd
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>()
                    .Handle<InvalidOperationException>(ex => 
                        ex.Message.Contains("temporarily unavailable")),
                OnRetry = args =>
                {
                    Console.WriteLine($"  ⏳ Reintento #{args.AttemptNumber} en {args.RetryDelay.TotalSeconds:F1}s " +
                                      $"(razón: {args.Outcome.Exception?.GetType().Name})");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Pipeline con reintentos para operaciones que retornan un valor.
    /// </summary>
    public static ResiliencePipeline<T> CreateRetryPipeline<T>(int maxRetries = 3)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = maxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>()
                    .Handle<InvalidOperationException>(ex => 
                        ex.Message.Contains("temporarily unavailable")),
                OnRetry = args =>
                {
                    Console.WriteLine($"  ⏳ Reintento #{args.AttemptNumber} en {args.RetryDelay.TotalSeconds:F1}s " +
                                      $"(razón: {args.Outcome.Exception?.GetType().Name})");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Pipeline avanzado: Retry + Timeout + Circuit Breaker.
    /// Para operaciones críticas que necesitan protección completa.
    /// </summary>
    public static ResiliencePipeline<T> CreateAdvancedPipeline<T>(
        int maxRetries = 3,
        TimeSpan? operationTimeout = null)
    {
        operationTimeout ??= TimeSpan.FromSeconds(10);

        return new ResiliencePipelineBuilder<T>()
            // 1. Timeout por operación individual
            .AddTimeout(operationTimeout.Value)
            // 2. Reintentos con backoff exponencial
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = maxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>()
                    .Handle<InvalidOperationException>(ex => 
                        ex.Message.Contains("temporarily unavailable")),
                OnRetry = args =>
                {
                    Console.WriteLine($"  ⏳ Reintento #{args.AttemptNumber} en {args.RetryDelay.TotalSeconds:F1}s");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}
