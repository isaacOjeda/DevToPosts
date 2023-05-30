using Auth0.AspNetCore.Authentication;
using Auth0Example.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
/**
Refs:
https://auth0.com/blog/call-protected-api-in-aspnet-core/
https://community.auth0.com/t/why-is-my-access-token-not-a-jwt-opaque-token/31028
https://github.com/auth0-samples/auth0-aspnetcore-mvc-samples
https://github.com/auth0-blog/call-protected-api-aspnet-core
https://auth0.com/blog/exploring-auth0-aspnet-core-authentication-sdk/
https://auth0.com/docs/quickstart/webapp/aspnet-core
*/

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHandler>();
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiHost"]);

})
.AddHttpMessageHandler<TokenHandler>();


builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    options.Scope = "openid profile email";

})
.WithAccessToken(options =>
{
    options.Audience = builder.Configuration["Auth0:Audience"];
});
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login";
});

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
//{
//    options.Cookie.Name = ".ClientWebAppAuth";
//})
//.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
//{
//    options.Authority = builder.Configuration["Auth0:Domain"];
//    options.ClientId = builder.Configuration["Auth0:ClientId"];
//    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
//    options.ResponseType = OpenIdConnectResponseType.Code;
//    options.TokenValidationParameters.NameClaimType = "name";
//    options.SaveTokens = true;

//    options.Events.OnRedirectToIdentityProviderForSignOut = (context) =>
//    {
//        var logoutUri = $"{builder.Configuration["Auth0:Domain"]}/v2/logout?client_id={builder.Configuration["Auth0:ClientId"]}";

//        var postLogoutUri = context.Properties.RedirectUri;
//        if (!string.IsNullOrEmpty(postLogoutUri))
//        {
//            if (postLogoutUri.StartsWith("/"))
//            {
//                // transform to absolute
//                var request = context.Request;
//                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
//            }
//            logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
//        }

//        context.Response.Redirect(logoutUri);
//        context.HandleResponse();

//        return Task.CompletedTask;
//    };
//    options.Events.OnRedirectToIdentityProvider = context =>
//    {
//        context.ProtocolMessage.SetParameter("audience", builder.Configuration["Auth0:Audience"]);

//        return Task.CompletedTask;
//    };
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages()
    .RequireAuthorization();

app.Run();
