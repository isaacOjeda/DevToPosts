using MultiTenants.Fx;
using MultiTenants.Fx.Contracts;

namespace MultiTenantMultiDatabase.MultiTenancy;
public class TenantAccessor : ITenantAccessor<Tenant>
{
    public TenantAccessor(IHttpContextAccessor contextAccessor, IConfiguration config, IWebHostEnvironment env)
    {
        Tenant = contextAccessor.HttpContext?.GetTenant();

        if (Tenant is null && env.IsDevelopment())
        {
            // Nota 👀: Si estamos en modo desarrollo y no hay Tenant, probablemente es alguna inicialización o creación de migración
            // en modo desarrollo
            Tenant = new Tenant(-1, "TBD");
            Tenant.Items["ConnectionString"] = config.GetConnectionString("SingleTenant");
        }
    }

    public Tenant? Tenant { get; init; }
}
