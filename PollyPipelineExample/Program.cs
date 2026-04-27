using Polly;
using PollyPipelineExample.Resilience;
using PollyPipelineExample.Services;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("   Polly v8 - ResiliencePipelineBuilder Demo");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

var unreliableService = new UnreliableService
{
    FailureRate = 0.7 // 70% de probabilidad de fallo
};

// Crear el pipeline de resiliencia
var retryPipeline = ResiliencePipelines.CreateRetryPipeline<string>(maxRetries: 4);

// ═══════════════════════════════════════════════════════════════
// Ejemplo 1: Operación CON resiliencia
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("📦 EJEMPLO 1: Operación CON pipeline de resiliencia");
Console.WriteLine("───────────────────────────────────────────────────────────────");

try
{
    unreliableService.ResetAttemptCount();

    var result = await retryPipeline.ExecuteAsync(
        async ct => await unreliableService.ProcessDataAsync("Datos importantes", ct),
        CancellationToken.None);

    Console.WriteLine($"\n🎉 Resultado final: {result}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"\n💀 Falló después de todos los reintentos: {ex.Message}\n");
}

// ═══════════════════════════════════════════════════════════════
// Ejemplo 2: Operación SIN resiliencia (para comparar)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("───────────────────────────────────────────────────────────────");
Console.WriteLine("📦 EJEMPLO 2: Operación SIN resiliencia (comparación)");
Console.WriteLine("───────────────────────────────────────────────────────────────");

try
{
    unreliableService.ResetAttemptCount();
    
    var result = await unreliableService.ProcessDataAsync("Datos sin protección");
    
    Console.WriteLine($"\n🎉 Resultado: {result}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"\n💀 Falló inmediatamente: {ex.GetType().Name} - {ex.Message}\n");
}

// ═══════════════════════════════════════════════════════════════
// Ejemplo 3: Pipeline inline (sin factory)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("───────────────────────────────────────────────────────────────");
Console.WriteLine("📦 EJEMPLO 3: Pipeline definido inline");
Console.WriteLine("───────────────────────────────────────────────────────────────");

var inlinePipeline = new ResiliencePipelineBuilder<string>()
    .AddRetry(new Polly.Retry.RetryStrategyOptions<string>
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromMilliseconds(500),
        ShouldHandle = new PredicateBuilder<string>()
            .Handle<Exception>(), // Maneja cualquier excepción
        OnRetry = args =>
        {
            Console.WriteLine($"  🔄 Retry #{args.AttemptNumber}...");
            return ValueTask.CompletedTask;
        }
    })
    .Build();

try
{
    unreliableService.ResetAttemptCount();
    unreliableService.FailureRate = 0.5; // Reducimos para este ejemplo

    var result = await inlinePipeline.ExecuteAsync(
        async ct => await unreliableService.ProcessDataAsync("Datos con pipeline inline", ct),
        CancellationToken.None);

    Console.WriteLine($"\n🎉 Resultado: {result}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"\n💀 Falló: {ex.Message}\n");
}

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("   Demo completada - Ejecuta varias veces para ver variaciones");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
