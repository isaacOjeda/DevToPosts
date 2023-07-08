using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Yarp.ReverseProxy.Transforms;

namespace Angular;

public static class Extensions
{
    public static IServiceCollection AddLocalAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".AngularJWTBFF";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

        return services;
    }

    public static IServiceCollection AddBffProxy(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(config.GetSection("ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(transformContext =>
                {
                    if (transformContext.HttpContext.User.Identity!.IsAuthenticated)
                    {
                        var accessTokenClaim = transformContext.HttpContext.User.Claims
                            .FirstOrDefault(q => q.Type == "Access_Token");

                        if (accessTokenClaim != null)
                        {
                            var accessToken = accessTokenClaim.Value;

                            transformContext.ProxyRequest.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", accessToken);
                        }
                    }

                    return ValueTask.CompletedTask;
                });
            });

        return services;
    }
}