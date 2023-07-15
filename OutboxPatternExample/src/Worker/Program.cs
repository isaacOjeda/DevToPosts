using ApplicationCore;

using Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostBuilder, services) =>
    {
        services.AddApplicationCore();
        services.AddInfrastructure(hostBuilder.Configuration);

        services.AddHostedService<OutboxProcessorWorker>();
    })
    .Build();

host.Run();
