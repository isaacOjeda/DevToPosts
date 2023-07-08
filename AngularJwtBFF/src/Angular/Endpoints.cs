using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Angular;


public static class Endpoints
{
    public const string LocalLogin = "/local-login";
    public const string LocalLogout = "/local-logout";


    public static void MapEndpoints(this IEndpointRouteBuilder routes, IConfiguration config)
    {
        routes.MapPost("/local-login", async (
            LoginRequest request,
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient();
            var baseAddress = config["ApiHost:Url"];
            var response = await client.PostAsJsonAsync($"{baseAddress}/api/token", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                var claims = new List<Claim>
                    {
                        new Claim("Access_Token", loginResponse!.Token)
                    };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await httpContext.SignInAsync(claimsPrincipal);

                // Leer el token y obtener los claims utilizando JWT
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(loginResponse.Token);

                return Results.Ok(new
                {
                    token.ValidTo,
                    Name = token.Claims.Where(q => q.Type == "unique_name").FirstOrDefault()?.Value,
                    Roles = token.Claims.Where(q => q.Type == "role").Select(q => q.Value)
                });
            }

            return Results.Forbid();
        });

        routes.MapPost("/local-logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync();
            return Results.Ok();
        });
    }

}


public record LoginRequest(string UserName, string Password);
public record LoginResponse(string Token);