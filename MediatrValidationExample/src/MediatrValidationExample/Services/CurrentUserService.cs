using MediatrExample.ApplicationCore.Common.Interfaces;
using System.Security.Claims;

namespace MediatrExample.WebApi.Services;
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        // Probablemente se está inicializando la aplicación.
        if (_httpContextAccessor is null || _httpContextAccessor.HttpContext is null)
        {
            User = new CurrentUser(Guid.Empty.ToString(), string.Empty, false);

            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext!.User!.Identity!.IsAuthenticated == false)
        {
            User = new CurrentUser(Guid.Empty.ToString(), string.Empty, false);

            return;
        }

        var id = httpContext.User.Claims
            .FirstOrDefault(q => q.Type == ClaimTypes.Sid)!
            .Value;

        var userName = httpContext.User!.Identity!.Name ?? "Unknown";

        User = new CurrentUser(id, userName, true);
    }

    public CurrentUser User { get; }

    public bool IsInRole(string roleName) =>
        _httpContextAccessor!.HttpContext!.User.IsInRole(roleName);
}
