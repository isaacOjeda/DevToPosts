
using Microsoft.EntityFrameworkCore;
using MultiTenants.Fx;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMultiTenancy(); // <--- custom middleware
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
