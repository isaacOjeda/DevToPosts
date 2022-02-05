using DomainEventsExample.Domain;
using DomainEventsExample.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DomainEventsExample.Persistence;

public class MyDbContext : DbContext
{
    private readonly IPublisher _publisher;
    private readonly ILogger<MyDbContext> _logger;

    public MyDbContext(
        DbContextOptions<MyDbContext> options,
        IPublisher publisher,
        ILogger<MyDbContext> logger) : base(options)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public DbSet<Product> Products => Set<Product>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        var events = ChangeTracker.Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .Where(domainEvent => !domainEvent.IsPublished)
                .ToArray();

        foreach (var @event in events)
        {
            @event.IsPublished = true;

            _logger.LogInformation("New domain event {Event}", @event.GetType().Name);

            await _publisher.Publish(@event);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>()
            .Ignore(x => x.DomainEvents);
    }
}
