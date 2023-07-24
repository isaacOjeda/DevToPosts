namespace MinimalApiScrutor.Features.Users;

public class CreateUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/users", (CreateUserRequest request) =>
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Password = request.Password
            };

            // TODO: Save user to database 

            return Results.Ok(new CreateUserResponse
            {
                UserId = user.Id
            });
        });

    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class CreateUserResponse
{
    public Guid UserId { get; set; } = default!;
}