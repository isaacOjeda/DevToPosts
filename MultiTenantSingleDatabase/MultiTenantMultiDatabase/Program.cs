using Microsoft.EntityFrameworkCore;
using MultiTenantMultiDatabase.MultiTenancy;
using MultiTenants.Fx;
using MultiTenants.Fx.Contracts;
using MultiTenantSingleDatabase.MultiTenancy;
using MultiTenantSingleDatabase.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMultiTenancy()
    .WithResolutionStrategy<HostResolutionStrategy>()
    .WithStore<DbContextTenantStore>();

builder.Services.AddDbContext<TenantAdminDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("TenantAdmin")));

builder.Services.AddDbContext<SingleTenantDbContext>();

builder.Services.AddTransient<ITenantAccessor<Tenant>, TenantAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMultiTenancy(); // <--- custom middleware
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
