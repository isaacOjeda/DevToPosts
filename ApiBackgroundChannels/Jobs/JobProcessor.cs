using System.Threading.Channels;

namespace ApiBackgroundChannels.Jobs;

public class JobProcessor : BackgroundService
{
    private readonly ILogger<JobProcessor> _logger;
    private readonly Channel<JobCommand> _channel;
    private bool _isJobRunning = false;
    private int _executionCount = 0;
    private DateTime? _lastExecutionTime;
    private CancellationTokenSource? _jobCancellationTokenSource;

    public JobProcessor(
        ILogger<JobProcessor> logger,
        Channel<JobCommand> channel)
    {
        _logger = logger;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobProcessor iniciado. Esperando comandos...");

        // Patrón recomendado por Microsoft: WaitToReadAsync + TryRead
        // Más eficiente que ReadAllAsync para escenarios de alta concurrencia
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_channel.Reader.TryRead(out var command))
            {
                try
                {
                    await ProcessCommandAsync(command, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando comando");
                    // Notificar error al productor si existe ResponseTask
                    command.ResponseTask?.TrySetException(ex);
                }
            }
        }
    }

    private async Task ProcessCommandAsync(JobCommand command, CancellationToken stoppingToken)
    {
        switch (command.Type)
        {
            case CommandType.Start:
                await StartJobAsync(command);
                break;

            case CommandType.Stop:
                await StopJobAsync(command);
                break;

            case CommandType.GetStatus:
                GetStatus(command);
                break;
        }
    }

    private async Task StartJobAsync(JobCommand command)
    {
        if (_isJobRunning)
        {
            _logger.LogWarning("El job ya está en ejecución");
            command.ResponseTask?.SetResult(new JobStatus
            {
                IsRunning = true,
                ExecutionCount = _executionCount,
                LastExecutionTime = _lastExecutionTime,
                Message = "El job ya está en ejecución"
            });
            return;
        }

        _isJobRunning = true;
        _jobCancellationTokenSource = new CancellationTokenSource();

        _logger.LogInformation("Iniciando job recurrente...");

        // Iniciar el trabajo recurrente en segundo plano
        _ = Task.Run(async () => await RunRecurringJobAsync(_jobCancellationTokenSource.Token));

        command.ResponseTask?.SetResult(new JobStatus
        {
            IsRunning = true,
            ExecutionCount = _executionCount,
            LastExecutionTime = _lastExecutionTime,
            Message = "Job iniciado exitosamente"
        });
    }

    private async Task StopJobAsync(JobCommand command)
    {
        if (!_isJobRunning)
        {
            _logger.LogWarning("El job no está en ejecución");
            command.ResponseTask?.SetResult(new JobStatus
            {
                IsRunning = false,
                ExecutionCount = _executionCount,
                LastExecutionTime = _lastExecutionTime,
                Message = "El job no está en ejecución"
            });
            return;
        }

        _isJobRunning = false;
        _jobCancellationTokenSource?.Cancel();
        _logger.LogInformation("Deteniendo job recurrente...");

        command.ResponseTask?.SetResult(new JobStatus
        {
            IsRunning = false,
            ExecutionCount = _executionCount,
            LastExecutionTime = _lastExecutionTime,
            Message = "Job detenido exitosamente"
        });
    }

    private void GetStatus(JobCommand command)
    {
        command.ResponseTask?.SetResult(new JobStatus
        {
            IsRunning = _isJobRunning,
            ExecutionCount = _executionCount,
            LastExecutionTime = _lastExecutionTime,
            Message = _isJobRunning ? "Job en ejecución" : "Job detenido"
        });
    }

    private async Task RunRecurringJobAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trabajo recurrente iniciado");

        while (!cancellationToken.IsCancellationRequested && _isJobRunning)
        {
            try
            {
                // Simular trabajo
                _executionCount++;
                _lastExecutionTime = DateTime.Now;

                _logger.LogInformation(
                    "Ejecutando trabajo #{Count} a las {Time}",
                    _executionCount,
                    _lastExecutionTime);

                // Aquí iría tu lógica de negocio
                // Por ejemplo: procesar datos, enviar notificaciones, etc.
                await Task.Delay(5000, cancellationToken); // Ejecutar cada 5 segundos
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Trabajo recurrente cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el trabajo recurrente");
                await Task.Delay(1000, cancellationToken); // Esperar antes de reintentar
            }
        }

        _logger.LogInformation("Trabajo recurrente finalizado");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("JobProcessor deteniéndose...");
        _jobCancellationTokenSource?.Cancel();
        await base.StopAsync(cancellationToken);
    }
}