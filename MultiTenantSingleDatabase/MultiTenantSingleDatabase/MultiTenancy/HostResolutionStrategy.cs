
using Microsoft.AspNetCore.Http;
using MultiTenants.Fx;
using System.Threading.Tasks;

namespace MultiTenantSingleDatabase.MultiTenancy;
public class HostResolutionStrategy : ITenantResolutionStrategy
{
    private readonly HttpContext? _httpContext;

    public HostResolutionStrategy(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext.HttpContext;
    }

    public async Task<string> GetTenantIdentifierAsync()
    {
        if (_httpContext is null)
        {
            return string.Empty;
        }

        return await Task.FromResult(_httpContext.Request.Host.Host);
    }
}
