using Angular;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBffProxy(builder.Configuration);
builder.Services.AddLocalAuthentication();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapEndpoints(builder.Configuration);
app.MapFallbackToFile("index.html");

app.Run();


