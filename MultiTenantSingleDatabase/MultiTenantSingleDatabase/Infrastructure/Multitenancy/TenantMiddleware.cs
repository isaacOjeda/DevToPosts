
using MultiTenantSingleDatabase.Helpers;

namespace MultiTenantSingleDatabase.Infrastructure.Multitenancy;
public class TenantMiddleware<T> where T : Tenant
{
    private readonly RequestDelegate next;

    public TenantMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Items.ContainsKey(AppConstants.HttpContextTenantKey))
        {
            var tenantStore = context.RequestServices.GetService(typeof(ITenantStore<T>)) as ITenantStore<T>;
            var resolutionStrategy = context.RequestServices.GetService(typeof(ITenantResolutionStrategy)) as ITenantResolutionStrategy;

            var identifier = await resolutionStrategy.GetTenantIdentifierAsync();
            var tenant = await tenantStore.GetTenantAsync(identifier);

            context.Items.Add(AppConstants.HttpContextTenantKey, tenant);
        }

        //Continue processing
        if (next != null)
            await next(context);
    }
}