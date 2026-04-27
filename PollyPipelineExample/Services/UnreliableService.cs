namespace PollyPipelineExample.Services;

/// <summary>
/// Simula un servicio externo que falla aleatoriamente.
/// Útil para demostrar patrones de resiliencia.
/// </summary>
public class UnreliableService
{
    private readonly Random _random = new();
    private int _attemptCount = 0;

    /// <summary>
    /// Probabilidad de fallo (0.0 a 1.0). Por defecto 70% de fallo.
    /// </summary>
    public double FailureRate { get; set; } = 0.7;

    /// <summary>
    /// Simula una operación que puede fallar (ej: subir archivo a la nube, llamar API externa, etc.)
    /// </summary>
    public async Task<string> ProcessDataAsync(string data, CancellationToken cancellationToken = default)
    {
        _attemptCount++;
        var currentAttempt = _attemptCount;

        Console.WriteLine($"  [Intento {currentAttempt}] Procesando: \"{data}\"...");

        // Simular latencia de red
        await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(100, 500)), cancellationToken);

        // Simular fallo aleatorio
        if (_random.NextDouble() < FailureRate)
        {
            var exception = GetRandomException();
            Console.WriteLine($"  [Intento {currentAttempt}] ❌ Falló con: {exception.GetType().Name}");
            throw exception;
        }

        Console.WriteLine($"  [Intento {currentAttempt}] ✅ Éxito!");
        return $"Procesado: {data} (intento #{currentAttempt})";
    }

    /// <summary>
    /// Reinicia el contador de intentos (útil entre operaciones).
    /// </summary>
    public void ResetAttemptCount() => _attemptCount = 0;

    private Exception GetRandomException()
    {
        var exceptions = new Exception[]
        {
            new HttpRequestException("Connection refused"),
            new TimeoutException("The operation timed out"),
            new InvalidOperationException("Service temporarily unavailable"),
            new IOException("Network error occurred")
        };

        return exceptions[_random.Next(exceptions.Length)];
    }
}
