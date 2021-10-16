using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MinimalApis.Api.Persistence;

namespace MinimalApis.Api.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo()
            {
                Description = "Minimal API Demo",
                Title = "Minimal API Demo",
                Version = "v1",
                Contact = new OpenApiContact()
                {
                    Name = "Isaac Ojeda",
                    Url = new Uri("https://github.com/isaacOjeda")
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {

        services.AddDbContext<MyDbContext>(options =>
            options.UseInMemoryDatabase(nameof(MyDbContext)));


        return services;
    }
}
