using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using MultiTenantApi.Data.Api;
using MultiTenantApi.Data.Tenants;

var builder = WebApplication.CreateBuilder(args);

// DB Context's
builder.Services.AddSqlServer<TenantsDbContext>(
    builder.Configuration.GetConnectionString("Tenants"));
builder.Services.AddDbContext<ApiDbContext>();

// Multitenancy support
builder.Services
    .AddMultiTenant<TenantInfo>()
    .WithHeaderStrategy("X-Tenant")
    .WithEFCoreStore<TenantsDbContext, TenantInfo>();

var app = builder.Build();

app.UseMultiTenant();

app.MapGet("/", (HttpContext httpContext) =>
{
    var tenantInfo = httpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;

    if (tenantInfo is null)
    {
        return Results.BadRequest();
    }

    return Results.Ok(new
    {
        tenantInfo.Identifier,
        tenantInfo.Id
    });
});

app.MapGet("/api/products", (ApiDbContext context) => 
    context.Products.ToListAsync());

await SeedTenantData();

app.Run();

async Task SeedTenantData()
{
    using var scope = app.Services.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();
    var tenants = await store.GetAllAsync();

    if (tenants.Count() > 0)
    {
        return;
    }

    await store.TryAddAsync(new TenantInfo
    {
        Id = Guid.NewGuid().ToString(),
        Identifier = "tenant01",
        Name = "My Dev Tenant 01",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=ApiMultiTenant_Tenant01;Trusted_Connection=True;MultipleActiveResultSets=true"
    });

    await store.TryAddAsync(new TenantInfo
    {
        Id = Guid.NewGuid().ToString(),
        Identifier = "tenant02",
        Name = "My Dev Tenant 2",
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=ApiMultiTenant_Tenant02;Trusted_Connection=True;MultipleActiveResultSets=true"
    });
}