namespace MultiTenants.Fx.Contracts;

public interface ITenantAccessor<T> where T : Tenant
{
    public T? Tenant { get; init; }
}
