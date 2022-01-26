# Introducción

En este post (un poco largo) veremos los conceptos principales de OpenID Connect y una implementación de ejemplo en ASP.NET Core.

Para la implementación de OpenID Connect en .NET utilizaremos [OpenIddict-core](https://github.com/openiddict/openiddict-core)  y .NET 6.

El ejemplo completo lo puedes ver en mi [GitHub](https://github.com/isaacOjeda/DevToPosts/tree/main/OpenIddictExample), te recomiendo que lo clones para una mejor comprensión del código que veremos aquí.

Puedes seguirme en Twitter en [@balunatic](https://twitter.com/balunatic) donde suelo poner cosas de programación, pero si tienes dudas o cualquier cosa, manda DM 😁. 

Este post se encuentra principalmente dividido en dos partes:

- Teoría de OpenID Connect

- Implementación del protocolo en ASP.NET Core

# OpenID Connect Protocol

## ¿Qué es OpenID Connect (OIDC)?

OpenID Connect (o OIDC) es un protocolo de identidad que utiliza los mecanismos de autenticación y autorización de OAuth 2.0. La especificación final de OIDC fue publicada en Febrero del 2014 y al día de hoy ha sido adoptado por una gran cantidad de proveedores de identidad.

OAuth 2.0 es un protocolo de autorización y OIDC es un protocolo de autenticación y es usado usado para verificar la identidad de un usuario en un servicio tercero (conocido como **Relying Party**).

Es decir, cuando haces una aplicación web y permites que tus usuarios inicien sesión con sus cuentas actuales como de **Google** o **Facebook**, es cuando se usa OIDC.

Tu aplicación web es ese **Relying Party** y esta aplicación no debe de tener las credenciales del usuario de Google o Facebook, ya que estamos buscando no exponer esa información y gracias OIDC no es necesario que las manipulemos. Siguiendo el flujo correcto, OIDC ayuda autenticar a un usuario sin que se tenga que crear una nueva cuenta en tu aplicación web y reutilizando cuentas existentes de servicios populares (de nuevo, como google o facebook).

Una gran variedad de clientes pueden usar OpenID Connect para autenticar usuarios, desde Single Page Applications (SPAs como Angular o React) hasta aplicaciones móviles nativas. También es usado en Single Sign On en muchas aplicaciones (SSO es muy útil cuando en tu organización tienen una gran variedad de servicios o aplicaciones internas y es necesario usar una misma cuenta).

## Diferencias entre OAuth 2.0 y OIDC

Realmente OIDC es funcionalidad adicional a la que ya existe en OAuth 2.0. Mientras OAuth 2.0 se trata del mecanismo para autorizar el acceso a información, OIDC se centra en la identidad del usuario (su autenticación). 

El propósito principal de OIDC es darte un solo lugar donde tengas que iniciar sesión sin importar la cantidad de sitios que tengas. Es decir, cuando tu accedes a tu aplicación que requiere un usuario autenticado, eres mandado a tu servidor de identidad (que utiliza OpenID) y ahí inicias sesión, eres redirigido de vuelta a la aplicación donde empezaste pero ya como un usuario autenticado. Las credenciales y tu información personal la tiene el proveedor de identidad y las aplicaciones de terceros solo necesitan saber si eres quien dices ser.

Sin embargo, OAuth se enfoca en proteger información y restringir el acceso. Por ejemplo: Cuando entras a myapp.com e inicias sesión con Facebook, se utiliza OpenID. Pero cuando en myapp.com te pregunta ¿Quieres importar tus contactos de Facebook a myapp.com? ahí se utilizaría OAuth, ya que Facebook te preguntará ¿Permitir a myapp.com que acceda a tu listado de contactos? y ese “consentimiento” es llevado a cabo siguiendo las reglas de OAuth.

## Elige el flujo correcto de autenticación

Algo que no he terminado de mencionar, es que OAuth y OIDC cuentan con varias formas de autenticar usuarios y es importante saber sus diferencias, porque dependiendo de la aplicación cliente, se utilizará un flujo diferente.

### Flujos no interactivos

Los flujos no interactivos no requieren que el usuario interactúe con el Authorization Server.

#### Resource owner password (no recomendado para aplicaciones nuevas)

Este flujo es directamente inspirado en el **basic autentication** (donde las credenciales se mandan en un header codificadas en base 64). En este caso, es el flujo más simple que existe en la especificación OAuth 2.0: La aplicación cliente (el Relying party) pregunta el usuario/contraseña, envía una solicitud de autorización al identity provider e inmediatamente regresa un access token si este es autorizado.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/4zfp4hv2g31xayhita38.png) 

> Nota 💡: Este flujo no se recomienda ya que el usuario y contraseña son directamente expuestos en la aplicación cliente. Por esta razón, este flujo no debe de ser usado cuando aplicaciones de terceros se ven involucrados. Si estas desarrollando aplicaciones internas (tu tienes el control de todo), por simplicidad, puede funcionar.

#### Client credentials grant (recomendado para comunicación machine-2-machine)

Este flujo podría decirse que es idéntico al anterior (resource owner password) pero este está diseñado para comunicación machine-2-machine (es decir, un servicio hablando con otro servicio) y ningún usuario se involucra en este flujo.

La aplicación cliente solicita el token enviando las credenciales y si estas son correctas, obtiene su `access_token` para acceder a los servicios requeridos.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/wblc4ezk9shkuar3i5ke.png) 

### Flujos interactivos

Estos flujos, como su nombre lo indica, requieren de interacción del usuario con el servidor de autenticación. De esta forma, las credenciales del usuario solo son usadas en el servidor de autenticación y ningún tercero las tiene que manipular.

#### Authorization code flow (recomendado para aplicaciones nuevas)

Este flujo probablemente es el más complicado de todos, ya que involucra redirecciones del navegador y comunicación con el backend. Sin embargo, es el más recomendado para cualquier escenario que involucre usuarios finales, ya que podrían iniciar sesión con sus credenciales, un PIN, un Smart Card o incluso usando otro proveedor externo.

Como ventaja de esta complejidad, el `access_token` jamás pasa por el navegador, ya que el backend hace ese intercambio de información directamente con el servidor de autenticación una vez que el usuario fue verificado.

Básicamente hay 2 pasos importantes en este flujo: La solicitud y respuesta al endpoint `authorization` y lo mismo con el endpoint `token`.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/3pnk6asr0ufwodpn6mg3.png) 

##### Authorization request

En este flujo, la aplicación cliente inicia el proceso de autenticación generando una solicitud de autorización incluyendo siempre el parámetro `response_type=code`, el `client_id`, el `redirect_uri` y opcionalmente, un `scope` y `state` (este último ayudando mitigar la vulnerabilidad de [ataques XSRF](https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest)).

Si la solicitud es válida, el servidor pedirá al usuario que se autentique y generalmente se pedirá el consentimiento de compartir información con la aplicación cliente (aunque esto depende directamente de la implementación del authorization server).

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/kmnafrlb7v4g03uty241.png) 

Si se inicia sesión y se da permiso, el navegador (user-agent) redirige de vuelta a la aplicación cliente incluyendo como parámetro en el URL un **authorization code** (este es un pequeño token, único y con una vida muy corta) y este se usa únicamente para intercambiar el código por un `access_token` y `id_token`.

##### Token request

Cuando la aplicación obtiene el authorization code, debe inmediatamente intercambiarlo por el access token y así dar por finalizado el proceso de autenticación.

Esto suena un proceso complicado, pero realmente siempre es igual, así que existen librerías o frameworks completos que ya hacen esto por nosotros.

#### Implicit flow

Implicit flow es muy similar al authorization code, excepto que no existe ese intercambio entre **authorization code** y los **tokens** (en el token request): el access token es directamente regresado al cliente como parte del proceso de autorización, es decir, en el redirect que existe desde el servidor de autorización y aplicación cliente, los tokens forman parte del URI.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/x9t8pg48ov8b7dtasolu.png) 

Este flujo típicamente es usado en aplicaciones frontend que no tienen backend que pueda hacer el intercambio del código recibido por el token.

Este flujo es menos seguro, porque los access token viajan por medio de un fragmento del URI y estos no se encuentran encriptados ni protegidos de ninguna forma.

Existen formas de prevenir ser vulnerable, pero la mejor opción es usar el flujo anterior utilizando el [Proof Key for Code Exchange](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow-with-proof-key-for-code-exchange-pkce).

### Existen más flujos que no es necesario aprender.

Realmente, los importantes son **client credentials** y **authorization code flow** y por simplicidad y si todo es interno, **resource owner password**. Existen otros como hybrid flow, device flow, etc. pero varios son obsoletos o no tan usados.

La siguiente tabla nos ayudará mejor a decidir:

| Type of Application   | OAuth 2.0 flow                                                                                                         |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Server-side (AKA Web) | Authorization Code flow                                                                                                |
| SPA                   | Authorization Code flow with PKCE o Implicit flow (solo si no hay compatibilidad en el navegador para usar Crypto Web) |
| Native                | Authorization Code flow with PKCE                                                                                      |
| Trusted               | Resource Owner Password flow                                                                                           |
| Service               | Client Credentials                                                                                                     |

# Implementando un Servidor de autenticación en ASP.NET Core

¿Acaso creen que ya habíamos terminado? Seguimos ahora con el código.

En este post veremos como crear 3 aplicaciones web: Servidor de autenticación (Identity Provider con OpenID), Aplicación web Cliente (El **Relying Party** que necesita de usuarios autenticados y acceder a recursos protegidos) y una Web API (resource protegido del usuario al que la aplicación web cliente quiere acceder).

## Proyecto IdentityServer: AKA Authorization Server

Para esto, necesitamos una aplicación web que nos administre usuarios. Para dejarlo simple, utilizaremos el template de asp.net core web con individual credentials que usa Identity Core:

```bash
dotnet new razor --auth individual --use-local-db true -o IdentityServer
```

Al usar cuentas individuales se utilizará Identity Core y el parámetro `local-db` simplemente indica que queremos usar SQLServer en lugar de SQLite (que es el default).

> Nota 💡: Para mejor referencia, puedes ver mi [repositorio de GitHub](https://github.com/isaacOjeda/DevToPosts/tree/main/OpenIddictExample) y ver este ejemplo completo.

Para este authorization server vamos a usar una muy buena librería llamada **Openiddict** (una solución más simple que **Identity Server de Duente**, pero completamente libre de usar).

Instalamos OpenIddict agregando sus paquetes:

```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="6.0.1" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.1" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="6.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.1" />
<!-- OpenIddict -->
<PackageReference Include="OpenIddict" Version="3.0.0" />
<PackageReference Include="OpenIddict.AspNetCore" Version="3.0.0" />
<PackageReference Include="OpenIddict.EntityFrameworkCore" Version="3.0.0" />
<!-- /OpenIddict -->
```

y comenzamos a configurar el servidor desde el **Program.cs**.

Hay que configurar **OpenIddict** para que use el `ApplicationDbContext` que por default nos agregó el template.

```csharp
using IdentityServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Register the entity sets needed by OpenIddict.
    options.UseOpenIddict();
});
```

OpenIddict guardará en SQLServer (usando EF Core) toda la información que necesita para aplicaciones clientes y sus tokens emitidos.

Después de eso, agregamos las dependencias de OpenIddict y los Flows permitidos:

```csharp
builder.Services.AddOpenIddict()
    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the EF Core stores/models.
        options
            .UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        options
            .AllowClientCredentialsFlow()
            .AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange()
            .AllowRefreshTokenFlow();

        options
            .SetTokenEndpointUris("/connect/token")
            .SetAuthorizationEndpointUris("/connect/authorize")
            .SetUserinfoEndpointUris("/connect/userinfo");

        // Encryption and signing of tokens
        options
            .AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey()
            .DisableAccessTokenEncryption();

        // Register scopes (permissions)
        options.RegisterScopes("api");
        options.RegisterScopes("profile");

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough();
    });
```

Aquí estamos haciendo lo siguiente:

- Estamos permitiendo el flujo de autorización **client credentials** y **authorization code,** cualquier flujo que se intente usar diferente, la solicitud será rechazada
  - También se está habilitando PKCE y de hecho se hace obligatorio implementarlo (por que es más seguro, especialmente en SPAs).
  - También se habilita la posibilidad de usar Refresh Tokens.
- Se están estableciendo las rutas para los endpoints:
  - **Token**. Para intercambiar el authorization code por access tokens y id tokens.
  - **Authorization**. Para solicitar el código de autorización después de haber iniciado sesión.
  - **User Info**. Para solicitar información adicional del usuario una vez autenticado.
- Con `AddEphemeralEncryptionKey`  y `AddEphemeralSigninKey` se genera una llave RSA asimétrica para modo desarrollo, ya que no se guarda en ningún lado y no se comparte entre instancias.
  - Nota: cada vez que reiniciar el el servidor, se generan llaves nuevas. Solo se usa para testing.
- Se registran los scopes que se usarán, estos funcionan como permisos.
- Los métodos Passthrough son para que nosotros podamos tomar acción de esos endpoints después de ser validados por OpenIddict.

Para finalizar con el contenido del **Program**, continúa con el siguiente código boilerplate.

```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapRazorPages();
app.MapControllers();

await SeedDefaultClients();

app.Run();
```

Inicialmente, necesitamos poder crear clientes de OpenID, por eso tenemos el método Seed que se describe a continuación:

```csharp
async Task SeedDefaultClients()
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    await context.Database.EnsureCreatedAsync();

    var client = await manager.FindByClientIdAsync("clientwebapp");

    if (client is null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "clientwebapp",
            ClientSecret = "client-web-app-secret",
            DisplayName = "ClientWebApp",
            RedirectUris = { new Uri("https://localhost:7003/signin-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        });
    }
}
```

Este Seed nos crea un cliente llamado **clientwebapp** y aquí mismo se especifican los permisos que tiene y su secret (que es necesario para validaciones futuras).

Habitualmente esto se podría hacer desde una UI, para dar de alta o de baja los clientes que desees usar, pero dejemos esto como un simple demo.

El template ya contiene UI para crear usuarios (registro) e inicio de sesión, todo utilizando la implementación default de Identity Core, por lo que esta parte no tenemos que implementar algo.

Lo que sí tenemos que hacer, es generar los claims (según el usuario autenticado) y validar las solicitudes que se reciben.

Para esto, crearemos el siguiente controller:

```csharp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace IdentityServer.Controllers;

public class AuthorizationController : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Create a new claims principal

        var claims = new List<Claim>
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim(OpenIddictConstants.Claims.Username, result.Principal.Identity.Name),
                new Claim(OpenIddictConstants.Claims.Audience, "test"),
            };

        var email = result.Principal.Claims.FirstOrDefault(q => q.Type == ClaimTypes.Email);
        if (email is not null)
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Email, email.Value));
        }

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set requested scopes (this is not done automatically)
        claimsPrincipal.SetScopes(request.GetScopes());

        foreach (var claim in claimsPrincipal.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                // If the "profile" scope was granted, allow the "name" claim to be
                // added to the access and identity tokens derived from the principal.
                OpenIddictConstants.Claims.Name when claimsPrincipal.HasScope(OpenIddictConstants.Scopes.Profile) => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                },

                // Never add the "secret_value" claim to access or identity tokens.
                // In this case, it will only be added to authorization codes,
                // refresh tokens and user/device codes, that are always encrypted.
                "secret_value" => Array.Empty<string>(),

                // Otherwise, add the claim to the access tokens only.
                _ => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken
                }
            });
        }

        // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal claimsPrincipal;

        if (request.IsClientCredentialsGrantType())
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Subject (sub) is a required field, we use the client id as the subject identifier here.
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

            // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
            identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

            claimsPrincipal = new ClaimsPrincipal(identity);

            claimsPrincipal.SetScopes(request.GetScopes());
        }
        else if (request.IsAuthorizationCodeGrantType())
        {
            // Retrieve the claims principal stored in the authorization code
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else
        {
            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        return Ok(new
        {
            Sub = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
            Name = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
            Occupation = "Developer",
            Age = 31
        });
    }
}
```

Te recomiendo que leas el código, investigues tus dudas y lo depures paso a paso para entender mejor el funcionamiento, pero aquí te va un resumen:

### Authorize

Este endpoint es el punto de entrada cuando se quiere autenticar. Con `GetOpenIddictServerRequest`se lee la solicitud OpenID y en caso de no ser válida, se lanza la excepción.

Si es válida, se verifica si el usuario actualmente está autenticado, sino, se manda a autenticar utilizando el inicio de sesión que nos ofrece Identity Core. El esquema de Cookies que agrega Identity Core es `IdentityConstants.ApplicationScheme` y ese es el esquema que usamos para mandarlo a iniciar sesión.

Una vez que inicia sesión, el `RedirectUri` lo regresa de vuelta a este método `Authorize()` y ahora el flujo continuará, generando los claims que desees, porque ya tenemos un usuario autenticado. 

Ha este punto OpenIddict ya validó la solicitud y validó los scopes, por eso pasamos a generar los claims y poner su destino.

El destino es importante, ya que el `access_token` es el que será usado por terceros, no debe de contener información delicada, justo como los comentarios lo describen.

### Exchange

Este método, como su nombre lo dice, sirve para intercambiar un **authorization code** por los tokens (tanto access y identity) en caso de que el flujo aplique.

En este ejemplo también se incluye el flujo clients credential, que solo se necesita el **client_id** y el **client_secret** (hablando del ejemplo machine-2-machine) para generar los tokens. Solo está ahí como referencias, si deseas hacer pruebas con este flow.

### User Info

Sinceramente, este es el que menos he probado, pero sirve para proveer información adicional al momento de que un usuario autorizado regrese a la aplicación cliente, más adelante veremos como decirle a ASP.NET que consulte también este endpoint después de un inicio de sesión exitoso.

### ¿Para qué sirve todo esto?

Lo que acabamos de hacer fue configurar nuestro Identity provider, si nos vamos al URL [https://localhost:7001/.well-known/openid-configuration](https://localhost:7001/.well-known/openid-configuration) (los puertos pueden variar) obtenemos la siguiente configuración:

```json
{
  "issuer": "https://localhost:7001/",
  "authorization_endpoint": "https://localhost:7001/connect/authorize",
  "token_endpoint": "https://localhost:7001/connect/token",
  "userinfo_endpoint": "https://localhost:7001/connect/userinfo",
  "jwks_uri": "https://localhost:7001/.well-known/jwks",
  "grant_types_supported": [
    "client_credentials",
    "authorization_code",
    "refresh_token"
  ],
  "response_types_supported": [
    "code"
  ],
  "response_modes_supported": [
    "form_post",
    "fragment",
    "query"
  ],
  "scopes_supported": [
    "openid",
    "offline_access",
    "api",
    "profile"
  ],
  "claims_supported": [
    "aud",
    "exp",
    "iat",
    "iss",
    "sub"
  ],
  "id_token_signing_alg_values_supported": [
    "RS256"
  ],
  "code_challenge_methods_supported": [
    "S256"
  ],
  "subject_types_supported": [
    "public"
  ],
  "token_endpoint_auth_methods_supported": [
    "client_secret_basic",
    "client_secret_post"
  ],
  "claims_parameter_supported": false,
  "request_parameter_supported": false,
  "request_uri_parameter_supported": false
}
```

Este endpoint sirve para explorar el Identity Provider, conocer sus endpoints, sus llaves públicas para las firmas de los tokens, los claims soportados, los scopes y tipos de flujos que soporta.

Este endpoint es usado por las aplicaciones clientes que utilizaran este servidor como su proveedor de identidad.

Lo que sigue ahora, es crear esa aplicación cliente.

## Proyecto WebClient: Aplicación Web Cliente.

Aquí crearemos una aplicación Razor de la misma forma que lo hicimos con el anterior, pero este, sin autenticación ni nada más:

```bash
dotnet new razor -o WebClient
```

> Nota 💡: Los URLs usados en la configuración del cliente es importante, ya que forma parte de las validaciones y si este no es correcto, el proceso de autorización será rechazado.

Para este proyecto necesitaremos el siguiente paquete:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="6.0.1" />
```

Esta aplicación de Razor pages trae el template default con bootstrap pero sin autenticación. Lo que vamos a hacer en el **Program.cs** es lo siguiente:

```csharp
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = ".ClientWebAppAuth";
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:7001";

    options.ClientId = "clientwebapp";
    options.ClientSecret = "client-web-app-secret";
    options.ResponseType = OpenIdConnectResponseType.Code;

    options.Scope.Add("api");
    options.Scope.Add("openid");
    options.Scope.Add("profile");

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.TokenValidationParameters.NameClaimType = "name";
});

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
```

Hay dos cosas importantes aquí, pero este es el resumen:

- `AddAuthentication`. Establece los esquemas de autenticación que por default se usarán en la aplicación.
- `AddCookie`. Agrega el esquema de autenticación por cookie, esto significa que una vez autenticado en este esquema, se generará una cookie de autenticación llamada **.ClientWebAppAuth.**
- `AddOpenIdConnect`. Aquí configuramos OpenID Connect y le estamos indicando que use el servidor de autenticación previamente creado:
  - **Authority**. Es el host del servidor de autenticación
  - **ClientId**, **Client secret** y **Response type**. Esta información debe de coincidir con el cliente creado por el Seed. Por ahora, solo permitimos `response_type=code` ya que en el proceso de autorización solo vamos a regresar el **authorization code,** para que este sea usado inmediatamente como ya lo vimos.
  - **Scope**. Aquí estamos agregando los scopes que necesitamos, no necesitas siempre todos, pero son los que la aplicación requiere (solo como ejemplo, se podrá no solicitar el scope de profile si no queremos información adicional del usuario).
  - `SaveTokens`. Esto indica que los tokens (access y identity) se guardarán en la cookie de auteneticación, esto para ser usados después (ejem. para llamar a la API protegida).
  - `GetClaimsFromUserInfoEndpoint`: Este flag indica si queremos consultar información adicional del usuario, estos se guardarán como claims en la cookie previamente configurada.
  - `TokenValidationParameters.NameClaimType`: Aquí simplemente le indicamos a [ASP.NET](http://ASP.NET) el nombre del claim que queremos que use para el nombre. Ejem. cuando usamos `User.Identity.Name`

Actualizamos **Index.cshtml** para poder ver los claims que se nos han emitido por el servidor de autorización:

```html
@page
@using Microsoft.AspNetCore.Authentication
@model IndexModel
@{
    ViewData["Title"] = "Home page";
}

<div class="text-center">
    <h1 class="display-4">Welcome</h1>
    <p>Learn about <a href="https://docs.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
</div>

@if(User.Identity!.IsAuthenticated)
{
    <h2>Welcome @User.Identity.Name</h2>

    <ul>
        @foreach(var claim in @User.Claims)
        {
            <li>@claim.Type: @claim.Value</li>
        }
        <li>access_token: @(await HttpContext.GetTokenAsync("access_token"))</li>
        <li>id_token: @(await HttpContext.GetTokenAsync("id_token"))</li>
    </ul>
}
```

### Probando la autenticación

Necesitamos correr el **IdentityServer** y el **WebClient** juntos. Al abrir **WebClient** este automáticamente nos redireccionará al **IdentityServer** para ser autenticados. Una vez finalizando el proceso, regresaremos a **WebClient** y tendremos el siguiente resultado:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/4f0h1qy7p3xwr2su6n30.png) 

Es importante mencionar que debemos de generar las migraciones de Entity Framework correspondientes y ejecutarlas (con `dotnet ef`) en el **IdentityServer**.

En este caso, como mi **IdentityServer** no tiene más información mía mas que mi correo, eso se está usando como mi nombre, pero podríamos extenderlo y agregar más información (esto, ya tema para más a rato referente a Identity Core)

## Proyecto ProtectedApi: La Web API protegida

Y por último, falta lo que queremos proteger, en este caso, una Web API.

Teóricamente, esta Web API contiene información privada del usuario autenticado, y para eso necesitamos un `access_token` para validar que usuarios nos acceden.

Por medio de JWT y Bearer authentication, usaremos los access tokens para validar cada solicitud que llega a nuestra Web API. Pero antes de empezar, hay que crear otro proyecto:

```bash
dotnet new web -o ProtectedApi
```

Para poder usar la autenticación por medio de JWT en nuestra API, necesitamos el siguiente paquete:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.1" />
```

El ejemplo será muy sencillo, tendremos solamente un endpoint en nuestra Web Api que requiere de un usuario autenticado:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
      .AddJwtBearer("Bearer", options =>
      {
          options.Authority = "https://localhost:7001";
          options.Audience = "IdentityServerWebClients";
      });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/me", (HttpRequest request) =>
{
    var user = request.HttpContext.User;

    return Results.Ok(new
    {
        Claims = user.Claims.Select(s => new
        {
            s.Type,
            s.Value
        }).ToList(),
        user.Identity.Name,
        user.Identity.IsAuthenticated,
        user.Identity.AuthenticationType
    });
})
.RequireAuthorization();

app.Run();
```

En este caso, el esquema default que se configura para la autenticación es **Bearer**. Estamos indicando quien es nuestro Identity Provider (authority) para que nos ayude a autenticar las solicitudes que se reciben.

Aquí ocurre la magia, por que **ProtectedApi** realmente no sabe nada de llaves privadas o públicas, pero las necesita para validar los access token que le llegan. En este caso, al indicar el host del authority, este automáticamente va y lee el endpoint `/.well-known` revisado anteriormente y lee las llaves públicas RSA que se necesitan para verificar los tokens.

**WebClient** tiene los access token, entonces necesitamos que este mismo intente acceder a **ProtectedApi** una vez autenticado.

Por lo tanto, agregamos la siguiente Razor page en **WebClient** llamada `Me.cshtml`:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace WebClient.Pages
{
    public class MeModel : PageModel
    {
        private readonly HttpClient _http;

        public MeModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        public string RawJson { get; set; } = default!;

        public async Task OnGet()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.GetAsync("https://localhost:7005/me");

            response.EnsureSuccessStatusCode();

            RawJson = await response.Content.ReadAsStringAsync();
        }
    }
}
```

Aquí por fines del ejemplo se está haciendo de una forma no muy práctica, pero lo que quiero dejar claro es como hacer una llamada HTTP a una API protegida con el access token otorgado por el Identity Provider (que a su vez, está guardada en el authorization cookie de **WebClient**).

Si consultamos esta página (navegando a `https://localhost:7003/me`) desplegamos el resultado visualizando el `RawJson`:

```html
@page
@model WebClient.Pages.MeModel

<h2>Calling Protected API result:</h2>

@Model.RawJson
```

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/ygvxdf1ecrzbiolvlxe1.png)

Si hacemos llamadas sin autenticar, o un JWT inválido, se regresará un `HTTP 401 Unauthorized`. 

# Conclusión

Esto pareciera un tema complicado, pero una vez que haces este ejemplo por ti mismo, puedes ver que sí es sencillo, ya que se hace una vez y de ahí funciona para autenticar N aplicaciones (agregando sus respectivos clientes).

Extenderlo, agregar más proveedores o más tipos de autenticación (ejemplo SAML) todo se hace en el **IdentityServer** y no hay nada más que cambiar en las demás aplicaciones, ya que los clientes hablan OpenID Connect y los claims y tokens están generados en base eso.

Como tarea, agrega Google o Facebook al **IdentityServer** para que pueda ser usado como opción también al iniciar sesión en **WebClient**.

Otra tarea que puedes realizar es agregar un cliente Javascript con backend empleando el patrón BFF (Backend For Frontend) o sin backend utilizando la librería helper [oidc-client-js](https://github.com/IdentityModel/oidc-client-js).

# Referencias

- [Getting started (openiddict.com)](https://documentation.openiddict.com/guides/getting-started.html)
- [Setting up an Authorization Server with OpenIddict - Part V - OpenID Connect - DEV Community 👩‍💻👨‍💻](https://dev.to/robinvanderknaap/setting-up-an-authorization-server-with-openiddict-part-v-openid-connect-a8j)
- [OpenID Connect Protocol (auth0.com)](https://auth0.com/docs/authenticate/protocols/openid-connect-protocol#openid-and-jwts)
- [What is OpenID Connect and what do you use it for? - Auth0](https://auth0.com/intro-to-iam/what-is-openid-connect-oidc/)
- [Authentication and Authorization Flows (auth0.com)](https://auth0.com/docs/get-started/authentication-and-authorization-flow)
- [Which OAuth 2.0 Flow Should I Use? (auth0.com)](https://auth0.com/docs/get-started/authentication-and-authorization-flow/which-oauth-2-0-flow-should-i-use)
- [OAuth 2.0 and OpenID Connect Overview | Okta Developer](https://developer.okta.com/docs/concepts/oauth-openid/#choosing-an-oauth-2-0-flow)
