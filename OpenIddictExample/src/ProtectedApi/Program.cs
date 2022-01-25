var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
      .AddJwtBearer("Bearer", options =>
      {
          options.Authority = "https://localhost:7001";
          options.Audience = "IdentityServerWebClients";
      });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/me", (HttpRequest request) =>
{
    var user = request.HttpContext.User;

    return Results.Ok(new
    {
        Claims = user.Claims.Select(s => new
        {
            s.Type,
            s.Value
        }).ToList(),
        user.Identity.Name,
        user.Identity.IsAuthenticated,
        user.Identity.AuthenticationType
    });
})
.RequireAuthorization();

app.Run();
