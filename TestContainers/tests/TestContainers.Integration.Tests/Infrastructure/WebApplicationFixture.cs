using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using TestContainers.Web.Data;

namespace TestContainers.Integration.Tests.Infrastructure;

public class WebApplicationFixture(SqlServerContainerFixture sqlServerContainerFixture)
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private Respawner? _respawner;

    async Task IAsyncLifetime.InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbSeed = scope.ServiceProvider.GetRequiredService<AppDbContextSeed>();

        // Initialize Respawner
        _respawner = await Respawner.CreateAsync(sqlServerContainerFixture.ConnectionString, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
        
        await dbSeed.EnsureCreatedAsync();
        await dbSeed.SeedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_respawner is null)
        {
            return;
        }

        await _respawner.ResetAsync(sqlServerContainerFixture.ConnectionString);
    }

    public AppDbContext DbContext => Services.GetRequiredService<AppDbContext>();
    public HttpClient HttpClient => CreateClient();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<AppDbContext>));

            services.Remove(descriptor!);

            var descriptorAppDbContext = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(AppDbContext));

            services.Remove(descriptorAppDbContext!);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(sqlServerContainerFixture.ConnectionString);
            });
        });
    }
}