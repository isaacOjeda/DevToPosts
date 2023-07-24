namespace MinimalApiScrutor.Features.Users;

public class GetUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/users", () => new List<User>
        {
            new User { Id = Guid.NewGuid(), Name = "User 1", Email = "user@mail.com"},
            new User { Id = Guid.NewGuid(), Name = "User 2", Email = "user2@mail.com"},
        });
    }
}