using System.Threading.Channels;
using ApiBackgroundChannels.Jobs;

namespace ApiBackgroundChannels.Endpoints;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var jobGroup = app.MapGroup("api/job")
            .WithTags("Job");

        /// <summary>
        /// Inicia el background job
        /// </summary>
        jobGroup.MapPost("start", async (
            ILogger<Program> logger,
            Channel<JobCommand> channel) =>
        {
            logger.LogInformation("Solicitud para iniciar job recibida");

            var tcs = new TaskCompletionSource<JobStatus>();
            var command = new JobCommand
            {
                Type = CommandType.Start,
                ResponseTask = tcs
            };

            // Enviar comando al Channel
            await channel.Writer.WriteAsync(command);

            // Esperar respuesta
            var status = await tcs.Task;
            return Results.Ok(status);
        })
        .WithName("StartJob");

        /// <summary>
        /// Detiene el background job
        /// </summary>
        jobGroup.MapPost("stop", async (
            ILogger<Program> logger,
            Channel<JobCommand> channel) =>
        {
            logger.LogInformation("Solicitud para detener job recibida");

            var tcs = new TaskCompletionSource<JobStatus>();
            var command = new JobCommand
            {
                Type = CommandType.Stop,
                ResponseTask = tcs
            };

            await channel.Writer.WriteAsync(command);

            var status = await tcs.Task;
            return Results.Ok(status);
        })
        .WithName("StopJob");

        /// <summary>
        /// Obtiene el estado actual del job
        /// </summary>
        jobGroup.MapGet("status", async (
            ILogger<Program> logger,
            Channel<JobCommand> channel) =>
        {
            logger.LogInformation("Solicitud de estado del job recibida");

            var tcs = new TaskCompletionSource<JobStatus>();
            var command = new JobCommand
            {
                Type = CommandType.GetStatus,
                ResponseTask = tcs
            };

            await channel.Writer.WriteAsync(command);

            var status = await tcs.Task;
            return Results.Ok(status);
        })
        .WithName("GetJobStatus");

        return app;
    }
}
