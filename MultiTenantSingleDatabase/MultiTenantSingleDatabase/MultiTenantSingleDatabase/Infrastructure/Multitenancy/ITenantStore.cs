﻿
namespace MultiTenantSingleDatabase.Infrastructure.Multitenancy;

public interface ITenantStore<T> where T : Tenant
{
    Task<T> GetTenantAsync(string identifier);
}