
# Introducción

En el mundo actual de las aplicaciones web, la seguridad y la autenticación son aspectos fundamentales para proteger los datos y garantizar la confianza de los usuarios. Una forma popular de implementar la autenticación en aplicaciones web es utilizando OpenID Connect, un protocolo de autenticación y autorización basado en OAuth 2.0.

En este contexto, el flujo de Code Flow de OpenID Connect desempeña un papel crucial al permitir la autenticación segura y la obtención de información del usuario en aplicaciones web. Este flujo de autorización establece una comunicación entre la aplicación cliente, el proveedor de identidad (como Auth0) y el servidor de recursos para verificar la identidad del usuario y proporcionar acceso a recursos protegidos.

En este tutorial, exploraremos en detalle el flujo de Code Flow de OpenID Connect y su implementación en ASP.NET Core. Aprenderemos cómo configurar una aplicación web y una API utilizando el patrón de autenticación por Bearer Tokens y cómo interactúan con un proveedor de identidad como Auth0. Veremos cómo ASP.NET Core consulta las claves públicas del proveedor de identidad para validar los JSON Web Tokens (JWT) emitidos, y cómo se garantiza la seguridad en cada paso del flujo.

Si estás interesado en mejorar la seguridad de tus aplicaciones web y brindar a tus usuarios una experiencia de autenticación robusta y confiable, ¡este tutorial te guiará a través de los pasos necesarios para implementar el flujo de Code Flow de OpenID Connect en ASP.NET Core!

# Autenticación con Auth0 (Open ID Connect)

Hoy utilizaremos una aplicación cliente realizada con Razor pages. Como es server-side la forma más fácil de utilizar OpenID Connect es con la autorización "Code Flow".

El flujo de Code Flow de OpenID Connect es un flujo de autorización utilizado para autenticar a los usuarios y obtener información sobre ellos en aplicaciones web. A continuación, se explica cómo funciona:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/c7lkswddj0i987qu58xg.png)


1. El cliente (aplicación web) redirige al usuario a la página de inicio de sesión de OpenID Connect en el proveedor de identidad (por ejemplo, Auth0).    
2. El proveedor de identidad autentica al usuario y le solicita su consentimiento para compartir ciertos datos con la aplicación cliente.
3. Una vez que el usuario ha sido autenticado y ha dado su consentimiento, el proveedor de identidad genera un código de autorización único y lo devuelve al cliente a través de una redirección.
4. El cliente recibe el código de autorización y realiza una solicitud de intercambio de código al proveedor de identidad para obtener un token de acceso y un token de actualización. Esta solicitud incluye el código de autorización, así como el identificador del cliente y el secreto del cliente para autenticar la solicitud.
5. El proveedor de identidad valida el código de autorización y, si es válido, emite un token de acceso y un token de actualización al cliente.
6. El Cliente generará una Cookie de autenticación en la que persistirá la información del usuario (Claims, JWT, etc), la cookie debe de estar encriptada y HTTP Only para que viaje sin problemas entre los servicios por la red.
7. El cliente utiliza el token de acceso  para realizar solicitudes protegidas en nombre del usuario autenticado. El token de acceso contiene información sobre el usuario y los alcances (scopes) concedidos.
7. Si el token de acceso expira y el cliente necesita acceder a recursos protegidos nuevamente, puede utilizar el token de actualización para obtener un nuevo token de acceso sin que el usuario tenga que autenticarse nuevamente.

En resumen, el flujo de Code Flow de OpenID Connect permite que una aplicación web obtenga tokens de acceso para autenticar y acceder a recursos protegidos en nombre del usuario. El proveedor de identidad autentica al usuario, emite un código de autorización y, después de un intercambio seguro, proporciona un token de acceso al cliente, que luego se utiliza para acceder a recursos protegidos en el servidor de recursos.

## ¿Por qué es importante usar servicios como Auth0?

Delegar la autenticación y autorización a servicios como Auth0 o Azure AD B2C ofrece beneficios significativos en términos de seguridad, facilidad de implementación, gestión simplificada, soporte para diferentes proveedores de identidad y protocolos, así como escalabilidad y rendimiento optimizados.

Estos servicios especializados garantizan una capa de seguridad sólida, reducen la carga de trabajo para los desarrolladores, permiten la integración rápida y eficiente, y ofrecen soporte para una amplia gama de proveedores de identidad (Facebook, Google, etc). Además, estos servicios son escalables y están diseñados para manejar grandes volúmenes de solicitudes de autenticación de manera eficiente. En resumen, delegar la autenticación y autorización a estos servicios mejora la seguridad, la experiencia del usuario y la eficiencia del desarrollo de aplicaciones web.

## Configuración de Auth0

Para utilizar Auth0 en nuestra aplicación, es necesario realizar una configuración adecuada en el panel de control de Auth0. A continuación, se explica cómo realizar esta configuración:

1. **Crea una cuenta en Auth0**: Comienza registrándote en Auth0 y crea una cuenta. Auth0 ofrece una opción gratuita que incluye funcionalidad básica y es suficiente para comenzar.
2. **Crea una aplicación**: Una vez que tienes una cuenta en Auth0, dirígete al panel de control y navega hasta la sección "Applications". Allí, puedes crear una nueva aplicación para tu aplicación web server-side. En el ejemplo del tutorial, se utiliza la opción "Regular Web App".

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/dxq8jal3cqb8d3uky9uf.png)

3. **Obtén los datos de configuración**: Dentro de la configuración de la aplicación en Auth0, encontrarás los siguientes datos importantes:
	- **Domain**: Este es el dominio asignado por Auth0 que utilizarás para iniciar sesión en tu aplicación. Puedes usar un dominio personalizado si lo deseas.
	- **Client ID**: Este ID es utilizado por la aplicación cliente para interactuar con Auth0.
	- **Client Secret**: Esta clave también es utilizada por la aplicación cliente para autenticarse con Auth0.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/a24jjtukuolqx638jm4h.png)

4. **Configura los URLs permitidos**: Es necesario configurar los URLs permitidos para tu aplicación. En este punto, puedes agregar "localhost" con el puerto asignado a tu aplicación. Ten en cuenta que esta configuración puede variar dependiendo del entorno de implementación.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/4sngw7ywsisd517jynry.png)

5. **Crea una API**: Además de la aplicación cliente, necesitarás crear una API en Auth0 para proteger los recursos. En la sección "Applications", puedes crear una nueva API y asignarle un identificador único. Este identificador será utilizado como `audience` durante el proceso de autorización.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/04tnyas09dofve6qweb0.png)


![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/72wmnbak41jt1yojrq03.png)

Es importante tener en cuenta que Auth0 ofrece un QuickStart incluido en su documentación, el cual es muy útil para comprender en detalle la configuración. Puedes seguirlo antes de continuar con el resto del tutorial para tener una mejor comprensión de los pasos de configuración.

## Proyecto Web

Comencemos creando un proyecto web utilizando la plantilla de Razor Pages. Abre una terminal y ejecuta el siguiente comando:

```bash
dotnet new webapp -o Auth0Example.Web
```

> Nota :💡 Recuerda que siempre puedes ver el código en este repositorio [DevToPosts/Auth0Example at main · isaacOjeda/DevToPosts (github.com)](https://github.com/isaacOjeda/DevToPosts/tree/main/Auth0Example)

Necesitamos agregar los paquetes de NuGet necesarios para la autenticación con Auth0:

```xml
    <PackageReference Include="Auth0.AspNetCore.Authentication" Version="1.2.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="7.0.4" />
```

Estos paquetes nos proporcionarán las herramientas necesarias para interactuar con Auth0 y realizar la autenticación.

### Autenticación con Auth0

Ahora vamos a configurar nuestra aplicación para que pueda autenticarse con Auth0. Para ello, necesitamos agregar la configuración en formato JSON en el archivo `appsettings.json`. Abre este archivo y añade el siguiente contenido:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "Auth0": {
    "Domain": "dev-kdgeunocq4sfkolh.us.auth0.com",
    "ClientId": "<ClientId>",
    "ClientSecret": "<Client Secret>",
    "Audience": "Protected.Api"
  },
  "ApiHost": "https://localhost:7085/"
}
```

Asegúrate de reemplazar `<ClientId>` y `<Client Secret>` con tus propios valores proporcionados por Auth0. Estos valores nos permitirán autenticar nuestra aplicación con Auth0.

Para configurar Auth0 utilizamos lo siguiente dentro de `Progam.cs`

```csharp
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
```

Este código agrega la autenticación de Auth0 al servicio de autenticación de ASP.NET Core. Utilizamos la configuración del archivo `appsettings.json` para establecer el dominio, el ClientId, el ClientSecret y la audiencia. Además, especificamos los scopes que deseamos solicitar durante la autenticación.

Con `WithAccessToken` estamos indicando que queremos obtener tokens de acceso para un "Audience", en este caso, la API protegida, sin esta parte, **Auth0 no generará access tokens**.

Ahora, configuraremos la autenticación por cookies:

```csharp
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login";
});
```

Este código configura la ruta de inicio de sesión en "/Login" para la autenticación por cookies, por default `AddAuth0WebAppAuthentication` agrega otra ruta utilizando Views y Controllers, pero aquí sin problema podemos poner la ruta que queramos usar para el Login.

A continuación, vamos a agregar un Http Handler que se encargará de incluir el Access Token en todas las llamadas HTTP salientes. Crea una nueva clase llamada `TokenHandler.cs` con el siguiente código:

```csharp
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
  
namespace Auth0Example.Web
  
public class TokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
  
    public TokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
  
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
  
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
  
        return await base.SendAsync(request, cancellationToken);
    }
}
```

Este código crea un `Http Handler` llamado `TokenHandler` que hereda de `DelegatingHandler`. El `TokenHandler` se utilizará para incluir el JWT en todas las llamadas HTTP salientes. El JWT se obtiene del contexto HTTP y se agrega como encabezado de autorización en la solicitud saliente.

Continuemos configurando más servicios y clientes HTTP en `Program.cs`. Agrega el siguiente código al método:

```csharp
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHandler>();
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiHost"]);
})
.AddHttpMessageHandler<TokenHandler>();
```

Este código agrega los servicios necesarios para las Razor Pages y registra el servicio `HttpContextAccessor` para acceder al contexto HTTP en otras partes del código. Además, se agrega el `TokenHandler` como un servicio `Scoped`. También configuramos un cliente HTTP llamado "Api" con una dirección base especificada en la configuración (este será la API que crearemos más adelante).

Además, agregamos el `TokenHandler` como un `HttpMessageHandler` para que se ejecute en cada solicitud HTTP realizada por este cliente.

Finalmente, vamos a configurar los middlewares necesarios para nuestra aplicación:

```csharp
var app = builder.Build();
  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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

Todo esto ya es muy default y habitual, lo único que es importante es que se especifica que todas las Razor Pages **requieren autorización para acceder a ellas.**

### Index, Login, Logout y SignedOut

Tendremos cuatro Razor Pages: Index (ya existe), Login, Logout y SignedOut.

#### Index

En esta página lo único que haremos es mostrar los Claims y la respuesta de la API protegida, solo para confirmar que todo el proceso funciona:

```html
@page
@using Microsoft.AspNetCore.Authentication
@model IndexModel
@{
    ViewData["Title"] = "Home page";
}
  
@if(User.Identity!.IsAuthenticated)
{
    <h2>Welcome @User.Identity.Name</h2>
    <img class="img-thumbnail" src="@(User.FindFirst("picture")?.Value)" />
    <ul>
        @foreach(var claim in @User.Claims)
        {
            <li>@claim.Type: @claim.Value</li>
        }
        <li>access_token: @(await HttpContext.GetTokenAsync("access_token"))</li>
        <li>id_token: @(await HttpContext.GetTokenAsync("id_token"))</li>
        <li>refresh_token: @(await HttpContext.GetTokenAsync("refresh_token"))</li>
    </ul>

  
    <h2>Raw Response from API</h2>
  
    @Model.RawApiRespons

}
```

El Claim `picture` es agregado por Auth0, el cual incluye el gravatar del correo electrónico usado.

Existen métodos de extensión como `GetTokenAsync` para acceder a los distintos tokens emitidos por Auth0.

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth0Example.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HttpClient _http;
  
    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _http = httpClientFactory.CreateClient("Api");
    }
    
    public string RawApiResponse { get; set; }

    public async Task OnGet()
    {
        var result = await _http.GetAsync("/me");
  
        result.EnsureSuccessStatusCode();
  
        RawApiResponse = await result.Content.ReadAsStringAsync();
    }
}
```

También habíamos configurado un `HttpClient`, este en automático va a incluir el JWT que API necesita para la autenticación por Bearer Tokens. El `TokenHandler` definido anteriormente se encarga de eso.

#### Login

Dentro de `Login.cshtml.cs`, el método `OnGet` maneja la solicitud GET y redirige al usuario a la página de inicio de sesión de Auth0 utilizando `ChallengeAsync`.

Cuando se llama a `ChallengeAsync` con el esquema de autenticación específico, como `Auth0Constants.AuthenticationScheme`, se inicia el flujo de autenticación para ese esquema, lo que nos llevaría a una redirección a Auth0 para iniciar sesión.

```csharp
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
  
namespace Auth0Example.Web.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public async Task OnGet(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();
  
            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
    }
}
```

#### Logout

Dentro de `Logout.cshtml.cs`, el método `OnGet` maneja la solicitud GET y realiza el proceso de cierre de sesión. Asegúrate de que la redirección después del cierre de sesión esté configurada correctamente en `WithRedirectUri` dentro del objeto `LogoutAuthenticationPropertiesBuilder`.

```csharp
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.RazorPages;
  
namespace Auth0Example.Web.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task OnGet()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri("/SignedOut")
                .Build();
  
            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
```

Se llama a `SignOutAsync` para cerrar la sesión del usuario tanto en Auth0 como en el esquema de autenticación de cookies.

#### SignedOut

Esta página no hace nada, solo muestra una página avisando que se ha cerrado sesión.

```html
@page
@model Auth0Example.Web.Pages.SignedOutModel
@{
}
<h1>Signed Out</h1>
```

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
  
namespace Auth0Example.Web.Pages
{
    [AllowAnonymous]
    public class SignedOutModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
```

## Proyecto API

Ahora, vamos a crear un proyecto de API para representar el recurso protegido al que solo se podrá acceder mediante el JWT emitido por Auth0:

```bash
dotnet new webapi -o Auth0Example.Api
```

La única dependencia que tendremos es el siguiente NuGet:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.0.0" />
```

Este paquete nos proporcionará las herramientas necesarias para la autenticación por Bearer Tokens en nuestro proyecto de API.

Ahora vamos a configurar la autenticación por Bearer Tokens dentro de `Program.cs` del proyecto de API. Agrega el siguiente código al método:

```csharp
var builder = WebApplication.CreateBuilder(args);
  
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
      .AddJwtBearer("Bearer", options =>
      {
          options.Authority = builder.Configuration["Auth0:Domain"];
          options.Audience = builder.Configuration["Auth0:Audience"];
      });
```

Este código configura la autenticación por Bearer Tokens en nuestro proyecto de API. Utilizamos la configuración del archivo `appsettings.json` para establecer el Authority y el Audience. Esto indica que nuestra API solo aceptará solicitudes que incluyan un JWT válido emitido por Auth0.

Aquí la configuración usada:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Auth0": {
    "Domain": "<Auth0 Domain>",
    "Audience": "Protected.Api",
    "UserInfoEndpoint": "<Auth0 Domain>/userinfo"
  }
}
```

Finalmente, vamos a configurar los middlewares necesarios para nuestra API en el método `Configure` del archivo `Program.cs`. Reemplaza el código existente del método `Configure` con el siguiente:

```csharp
var app = builder.Build();
  
app.UseAuthentication();
app.UseAuthorization();

// Endpoints aquí

app.Run();
```

Este código configura los middlewares necesarios para nuestra API. Incluye la autenticación y la autorización. Estos endpoints van antes de `app.Run();`

```csharp
app.MapGet("/", () => "Hello World!");
  
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
```

El primer endpoint ("/") simplemente devuelve el mensaje "Hello World!", siempre lo hago para confirmar que la API corre sin problema.

El segundo endpoint ("/me") devuelve información sobre el usuario autenticado, incluyendo las claims del usuario, el nombre de usuario, si está autenticado y el tipo de autenticación utilizada. Además, requerimos que el usuario esté autenticado para acceder a este endpoint mediante el método `RequireAuthorization()`.

### ¿Cómo se validan los JWT?

ASP.NET Core utiliza el mecanismo de descubrimiento de claves públicas JSON Web Key Set (JWKS) para consultar y validar las firmas de los JSON Web Tokens (JWT) emitidos por Auth0. A través del descubrimiento de claves públicas, ASP.NET Core puede verificar la autenticidad de los JWT y asegurarse de que hayan sido emitidos por Auth0.

Cuando configuramos la autenticación por Bearer Tokens en ASP.NET Core, establecemos el `Authority` en la configuración del `JwtBearerOptions`. Esta propiedad indica la URL base donde ASP.NET Core puede consultar el JWKS de Auth0.

Cuando una solicitud llega a nuestra aplicación ASP.NET Core con un JWT en el encabezado de autorización, el middleware de autenticación realiza automáticamente los siguientes pasos:

1. **Verifica la firma del JWT:** El middleware extrae el JWT del encabezado de autorización y verifica su firma utilizando las claves públicas obtenidas del JWKS.
2. **Valida la audiencia y el emisor**: El middleware valida que el JWT esté destinado a nuestra aplicación y que haya sido emitido por el emisor esperado (Auth0) utilizando la información proporcionada durante la configuración.
3. **Valida la fecha y la hora**: El middleware verifica que el JWT no haya caducado y que no se haya emitido en el futuro.
4. **Extrae las claims del JWT**: Si la validación es exitosa, el middleware extrae las claims del JWT y las agrega al contexto de la solicitud. Esto permite que nuestras aplicaciones accedan a la información del usuario autenticado y tomen decisiones basadas en esas claims.

Para realizar el descubrimiento de claves públicas, ASP.NET Core envía una solicitud HTTP GET al endpoint `/.well-known/jwks.json` en el `Authority` especificado. Auth0 responde con un documento JSON que contiene las claves públicas necesarias para verificar los JWT emitidos. Estas claves públicas se actualizan periódicamente y se pueden rotar, por lo que ASP.NET Core realiza el descubrimiento de claves cada vez que se recibe un JWT para asegurarse de utilizar las claves más actualizadas.

> Nota 💡: También tengo entendido que estas llaves se guardan en caché por cierto tiempo, ya que no cambian a cada momento y por cuestiones de performance, se cachean.

En resumen, ASP.NET Core consulta el JWKS de Auth0 a través de la URL proporcionada en el `Authority` para obtener las claves públicas necesarias y validar las firmas de los JWT emitidos por Auth0. Esto permite que nuestra aplicación pueda verificar la autenticidad y la integridad de los tokens y brinda un nivel de seguridad adicional al proceso de autenticación.

## Probando la solución

Para probar solo hay que correr las dos aplicaciones, yo lo hago con `dotnet run -lp https` para que el `Launch Profile` sea `https` (esto viene en `launchSettings.json`, puede que sea diferente para ti).

Como la aplicación web necesita estar autenticado para acceder a cualquier `Razor Page`, automáticamente el middleware de Cookie Authentication nos mandará a la página `/Login` el cual hará la redirección a Auth0.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/yqv6ar59qcx6dnd9gufm.png)

Al crear una cuenta o iniciar sesión, si es la primera vez que lo hacemos, se nos pedirá el consentimiento hablado en el flujo:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/anitzxksbwqan85trgec.png)

Al aceptar, se redireccionará a la página `Index`

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/jkw35j3cwlbstg8j927g.png)

Aquí confirmamos que todo el flujo funciona y sale un Isaac Ojeda de 23 años en la foto (necesito actualizar mi Gravatar jaja).

# Conclusión

En esta entrada, hemos explorado el flujo de Code Flow de OpenID Connect y su implementación en ASP.NET Core. Hemos comprendido cómo este flujo de autorización permite la autenticación segura de usuarios y la obtención de información del usuario en aplicaciones web.

A lo largo del tutorial, aprendimos a configurar una aplicación web y una API utilizando el patrón de autenticación por Bearer Tokens, y cómo interactuar con un proveedor de identidad como Auth0 para autenticar a los usuarios y obtener tokens de acceso.

También comprendimos cómo ASP.NET Core utiliza el descubrimiento de claves públicas JSON Web Key Set (JWKS) para validar los JWT emitidos por el proveedor de identidad. Este proceso de validación garantiza la autenticidad y la integridad de los tokens, brindando un nivel adicional de seguridad a nuestras aplicaciones.

Al implementar el flujo de Code Flow de OpenID Connect en ASP.NET Core, hemos fortalecido la seguridad de nuestras aplicaciones web al requerir una autenticación adecuada y proteger los recursos sensibles de nuestros usuarios.

Recuerda que la seguridad es un aspecto crítico en cualquier aplicación web, y el uso de estándares como OpenID Connect nos permite aprovechar tecnologías robustas y probadas para garantizar la protección de los datos y la confianza de los usuarios.

Esperamos que este tutorial haya sido útil y te haya brindado una comprensión sólida del flujo de Code Flow de OpenID Connect en ASP.NET Core. ¡Ahora estás listo para aplicar estos conocimientos y mejorar la seguridad de tus propias aplicaciones web!

# Referencias

- [Call Protected APIs in ASP.NET Core (auth0.com)](https://auth0.com/blog/call-protected-api-in-aspnet-core/)
- [auth0-samples/auth0-aspnetcore-mvc-samples: Auth0 Integration Samples for ASP.NET Core MVC Web Applications (github.com)](https://github.com/auth0-samples/auth0-aspnetcore-mvc-samples)
- [auth0-blog/call-protected-api-aspnet-core (github.com)](https://github.com/auth0-blog/call-protected-api-aspnet-core)
- [Exploring the Auth0 ASP.NET Core Authentication SDK](https://auth0.com/blog/exploring-auth0-aspnet-core-authentication-sdk/)
- [Authentication and Authorization Flows (auth0.com)](https://auth0.com/docs/get-started/authentication-and-authorization-flow)
