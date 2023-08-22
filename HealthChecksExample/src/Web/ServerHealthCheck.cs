using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web;

public class ServerHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Aquí puedes agregar tu lógica para verificar el estado del servidor.
        // Por ejemplo, puedes verificar si el servidor responde a una solicitud de ping.

        var isServerHealthy = CheckServerStatus();

        return Task.FromResult(isServerHealthy
            ? HealthCheckResult.Healthy("El servidor está en funcionamiento.")
            : HealthCheckResult.Unhealthy("El servidor no está respondiendo."));
    }

    private bool CheckServerStatus()
    {
        // Aquí puedes implementar la lógica real para verificar el estado del servidor.
        // Por ejemplo, podrías intentar hacer una solicitud de ping al servidor y verificar la respuesta.

        // En este ejemplo, simplemente vamos a simular que el servidor está en funcionamiento.
        return true;
    }
}