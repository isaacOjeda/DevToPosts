
# Introducción

En el mundo del desarrollo web, la implementación de un patrón Backend for Frontend (BFF) se ha vuelto cada vez más relevante. El BFF permite optimizar la comunicación entre el frontend y el backend al proporcionar una capa intermedia que se adapta específicamente a las necesidades del cliente. En este artículo, exploraremos la implementación del patrón BFF utilizando Angular como aplicación cliente y ASP.NET Core junto con YARP como backend. Además, veremos cómo establecer autenticación y autorización en ambos lados: en el BFF y en el backend API.

> Nota 💡: Aquí encontrarás el código fuente de este post: [DevToPosts/AngularJwtBFF](https://github.com/isaacOjeda/DevToPosts/tree/main/AngularJwtBFF)

## Backend for Frontend

Antes de sumergirnos en la implementación, es importante comprender el patrón Backend for Frontend. El BFF es una capa intermedia que se sitúa entre el frontend y el backend principal, diseñada para adaptarse específicamente a las necesidades del cliente. Proporciona una interfaz API más especializada y optimizada para el frontend, reduciendo la complejidad y mejorando el rendimiento de la comunicación.

Para implementar el BFF, utilizaremos Angular como nuestra aplicación cliente y ASP.NET Core junto con YARP como backend. Angular actuará como el cliente principal, mientras que el BFF construido con ASP.NET Core y YARP se encargará de interactuar con una API protegida.

En el BFF construido junto con Angular, implementaremos la autenticación y autorización utilizando cookies. Las cookies son un mecanismo seguro para almacenar información de autenticación en el cliente. Angular manejará la autenticación del usuario y el BFF establecerá una cookie segura que contendrá un JWT (JSON Web Token) encriptado. Esta cookie será enviada en cada solicitud al BFF.

La API protegida también estará construido con ASP.NET Core y proporcionará servicios y recursos protegidos. Para la autenticación en el backend API, utilizaremos el esquema de autenticación por Bearer Tokens. Cuando el BFF recibe una solicitud autenticada con una cookie, extraerá el JWT encriptado y lo incluirá como un token (Bearer token) en la solicitud hacia el backend API.

### Reverse Proxy (Con YARP)

YARP (Yet Another Reverse Proxy) es una biblioteca de .NET que nos permitirá redirigir y transformar las peticiones del BFF hacia el backend API. Utilizaremos YARP para asegurarnos de que el JWT encriptado en la cookie se envíe correctamente al backend API y mantener la seguridad de la comunicación.

## Importancia de implementar el BFF en aplicaciones SPA

Las aplicaciones SPA a menudo utilizan el almacenamiento local en el navegador, como el **localStorage**, para guardar el JSON Web Token (JWT) que contiene la información de autenticación del usuario. Sin embargo, este enfoque puede ser inseguro. Los ataques de tipo XSS (Cross-Site Scripting) y las técnicas de phishing pueden permitir a un atacante acceder al contenido almacenado en el navegador y extraer el JWT, lo que les permitiría suplantar la identidad del usuario.

Al implementar el BFF, se establece una capa intermedia entre el frontend y el backend principal. Esto permite controlar y mitigar los riesgos de seguridad asociados con el manejo del JWT. En lugar de almacenar el JWT directamente en el cliente, el BFF puede utilizar mecanismos más seguros, como cookies seguras, para almacenar y transmitir la información de autenticación. Esto reduce significativamente la exposición del JWT a posibles ataques.

El BFF ofrece una separación clara de responsabilidades entre el frontend y el backend. El frontend se centra en la experiencia del usuario y la presentación de datos, mientras que el BFF se encarga de las operaciones específicas del cliente, como la autenticación y autorización. Esta separación mejora la modularidad y la escalabilidad del sistema, ya que cada componente puede evolucionar de forma independiente.

### Flujo de la aplicación

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/tfx35asyqzj1vpd6tu6p.png)
1. El cliente Angular envía una solicitud al Backend for Frontend (BFF) junto con una cookie que contiene la información de autenticación.
2. El BFF recibe la solicitud del cliente Angular y puede acceder a la cookie que contiene el token de autenticación.
3. El BFF reenvía la solicitud a la API.
4. La API verifica el token de autenticación Bearer incluido en la solicitud enviada por el BFF.
5. Después de verificar el token, la API envía una respuesta al BFF.
6. El BFF recibe la respuesta de la API y la reenvía al cliente Angular.
7. El cliente Angular recibe la respuesta del BFF y puede mostrar los datos o realizar otras acciones según sea necesario.

En este diagrama, se destaca la comunicación entre Angular, el BFF y la API protegida. El cliente Angular envía una solicitud con una cookie que contiene la información de autenticación al BFF. El BFF, al recibir la solicitud, puede acceder a la cookie y extraer el token de autenticación para incluirlo en la solicitud hacia la API. La API, a su vez, verifica el token de autenticación Bearer y envía una respuesta al BFF. El BFF reenvía la respuesta al cliente Angular, quien la procesa y muestra los datos correspondientes.

Este enfoque permite una comunicación segura y eficiente entre el cliente Angular, el BFF y la API protegida. El uso de una cookie para la autenticación en el lado del cliente y la autenticación por Bearer Token en el lado de la API garantiza la seguridad de la información de autenticación y protege los recursos protegidos de accesos no autorizados.

## Proyecto API

Comenzaremos creando un nuevo proyecto de API en ASP.NET Core. Abre una terminal y ejecuta el siguiente comando:

```bash
dotnet new webapi -o API
```

Este comando generará un nuevo proyecto de API en una carpeta llamada "API".

> 💡Nota: El código lo puedes encontrar aquí ([DevToPosts/AngularJwtBFF](https://github.com/isaacOjeda/DevToPosts/tree/main/AngularJwtBFF)) por si no quieres escribir todo, te recomiendo que lo veas para tener una mejor referencia

Ahora, configuraremos la autenticación y definiremos los endpoints necesarios para autenticar usuarios y acceder a los recursos protegidos.

En el proyecto API, crea un archivo llamado `Constants.cs` y agrega el siguiente código:

```csharp
namespace Api;
  
public static class Constants
{
    public const string SECRET_KEY = "This is my custom Secret key...";
    public const string ISSUER = "http://localhost:5000";
}
```

En este archivo, definimos dos constantes: `SECRET_KEY` y `ISSUER`. La `SECRET_KEY` se utilizará para firmar y verificar los tokens JWT, y el `ISSUER` representa el emisor del token.

> Nota 💡: Esto es un ejemplo, siempre hay que llevarnos esta información a un lugar más seguro y no visible en el código.

Crea un archivo llamado `Endpoints.cs` y agrega el siguiente código:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
  
namespace Api;
  
public static class Endpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/token", (LoginRequest request) =>
        {
            if (request.Password != "admin")
            {
                return Results.Unauthorized();
            }

            // Genera un JWT dummy
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Constants.SECRET_KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, request.UserName),
                    new Claim(ClaimTypes.Email, $"{request.UserName}@localhost"),
                    new Claim(ClaimTypes.Role, "Administrator"),
                    new Claim(ClaimTypes.Role, "OtherRole")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = Constants.ISSUER
            };
  
            var token = tokenHandler.CreateToken(tokenDescriptor);
  
            return Results.Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        });
  
        app.MapGet("/api/claims", (HttpContext http) =>
        {
            var claims = http.User.Claims.Select(c => new { c.Type, c.Value });
            return claims;
        }).RequireAuthorization();
  
        app.MapGet("/api/products", () =>
        {
            var products = new[]
            {
                new { Id = 1, Name = "Product 1" },
                new { Id = 2, Name = "Product 2" },
                new { Id = 3, Name = "Product 3" },
                new { Id = 4, Name = "Product 4" },
                new { Id = 5, Name = "Product 5" },
            };
            return products;
        }).RequireAuthorization();
    }
}

  
public record LoginRequest(string UserName, string Password);
```

En este archivo, creamos los endpoints de la API. El endpoint `/api/token` permite autenticar usuarios y generar un token JWT. El endpoint `/api/claims` devuelve pues, los claims del usuario autenticado. El endpoint `/api/products` devuelve una lista de productos como ejemplo de un recurso protegido.

Crea un archivo llamado `Extensions.cs` y agrega el siguiente código:

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
  
namespace Api;

  
public static class Extensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddAuthorization();
  
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Constants.ISSUER,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.SECRET_KEY))
                };
            });
  
        return services;
    }
}
```

En este archivo, definimos una extensión de `IServiceCollection` llamada `AddApiAuthentication` para configurar la autenticación en la API. Configuramos el esquema de autenticación JWT Bearer y establecemos los parámetros de validación del token.

En el archivo `Program.cs`, reemplaza el código existente con el siguiente código:

```csharp
using Api;
  
var builder = WebApplication.CreateBuilder(args);
  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiAuthentication();
  
var app = builder.Build();
  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
  
app.UseAuthentication();
app.UseAuthorization();
  
app.MapEndpoints();
  
app.Run();
```

Hasta aquí ya tenemos la API protegida, podemos hacer pruebas con postman o swagger para confirmar que la autenticación y autorización funcionan.

> Nota 💡: En el repositorio tengo un archivo `api.http` que uso para probar dentro del mismo VS Code, swagger me gusta más para generar clientes http pero para probar me gusta usar más esta extensión [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

## Proyecto Angular

Nuevamente en la terminal, ejecuta el siguiente comando para crear un proyecto Angular:

```bash
dotnet new angular -o Angular
```

Este comando generará un nuevo proyecto Angular en una carpeta llamada "Angular".

> Nota 💡: Recuerda que esta plantilla genera un proyecto en ASP.NET Core que hospeda una aplicación frontend angular, así que aprovecharemos eso (osea, ya tenemos un backend).

### BFF (El backend de Angular)

En el proyecto que acabamos de crear, crea un archivo llamado `Extensions.cs` y agrega el siguiente código:

```csharp
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Yarp.ReverseProxy.Transforms;
  
namespace Angular;
  
public static class Extensions
{
    public static IServiceCollection AddLocalAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".AngularJWTBFF";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
  
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });
  
        return services;
    }
  
    public static IServiceCollection AddBffProxy(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(config.GetSection("ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(transformContext =>
                {
                    if (transformContext.HttpContext.User.Identity!.IsAuthenticated)
                    {
                        var accessTokenClaim = transformContext.HttpContext.User.Claims
                            .FirstOrDefault(q => q.Type == "Access_Token");
  
                        if (accessTokenClaim != null)
                        {
                            var accessToken = accessTokenClaim.Value;
  
                            transformContext.ProxyRequest.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", accessToken);
                        }
                    }
  
                    return ValueTask.CompletedTask;
                });
            });
  
        return services

    }

}
```

En este archivo, definimos dos métodos de extensión para `IServiceCollection`. El método `AddLocalAuthentication` configura la autenticación en el BFF utilizando cookies. Se establece el esquema de autenticación de cookies y se configuran algunas opciones de cookies, como el nombre, la política de seguridad y la respuesta en caso de acceso denegado.

> Nota 💡: Sobreescribimos `OnRedirectToAccessDenied` porque el comportamiento default de autenticación por cookies es la redirección a una página predeterminada (ejem `/AccessDenied`) y como no estamos usando Razor Pages o similar, no queremos una redirección, sino el error HTTP 403.

El método `AddBffProxy` configura el _reverse proxy_ en el BFF utilizando YARP (Yet Another Reverse Proxy). Carga la configuración de YARP desde la sección `ReverseProxy` del archivo de configuración y agrega una transformación para incluir el token de acceso (Bearer Token) en las solicitudes al backend.

Aquí es donde ocurre la magia, en Angular tenemos una autenticación por Cookies pero en API tenemos Bearer Tokens. Por lo que se hace este intercambio, en la Cookie se guarda el JWT (lo vemos en el siguiente código) y de esta forma el JWT no es accesible en el frontend de ninguna forma, ya que es HTTP Only y aparte está encriptado.

En el archivo `Endpoints.cs`, agrega el siguiente código:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
  
namespace Angular;

public static class Endpoints
{
    public const string LocalLogin = "/local-login";
    public const string LocalLogout = "/local-logout";

  
    public static void MapEndpoints(this IEndpointRouteBuilder routes, IConfiguration config)
    {
        routes.MapPost("/local-login", async (
            LoginRequest request,
            HttpContext httpContext,
            IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient();
            var baseAddress = config["ApiHost:Url"];
            var response = await client.PostAsJsonAsync($"{baseAddress}/api/token", request);
  
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
  
                var claims = new List<Claim>
                    {
                        new Claim("Access_Token", loginResponse!.Token)
                    };
  
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
  
                await httpContext.SignInAsync(claimsPrincipal);
  
                // Leer el token y obtener los claims utilizando JWT
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(loginResponse.Token);
  
                return Results.Ok(new
                {
                    token.ValidTo,
                    Name = token.Claims.Where(q => q.Type == "unique_name").FirstOrDefault()?.Value,
                    Roles = token.Claims.Where(q => q.Type == "role").Select(q => q.Value)
                });
            }
  
            return Results.Forbid();
        });
  
        routes.MapPost("/local-logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync();
            return Results.Ok();
        });
    }
  
}

  
public record LoginRequest(string UserName, string Password)

public record LoginResponse(string Token);
```

En este archivo, definimos dos endpoints en el BFF: `/local-login` y `/local-logout`. El endpoint `/local-login` permite a los usuarios iniciar sesión enviando una solicitud POST con las credenciales de inicio de sesión. El BFF envía las credenciales al backend para obtener un token de acceso (JWT). Si la respuesta es exitosa, se crea una identidad de usuario con el token de acceso y se inicia sesión utilizando el esquema de autenticación de cookies. Además, se extraen algunos datos del JWT y se devuelven en la respuesta.

El endpoint `/local-logout` permite a los usuarios cerrar sesión y se encarga de cerrar la sesión actual del usuario, lo que ocasiona borrar las cookies de autenticación.

Y para terminar:

```csharp
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
```

En este archivo, configuramos los servicios y el middleware necesarios para el BFF. Utilizamos los métodos de extensión `AddBffProxy` y `AddLocalAuthentication` que definimos anteriormente. También agregamos el servicio `IHttpClientFactory` para realizar solicitudes HTTP al backend. Configuramos el middleware para el reverse proxy y mapeamos los endpoints definidos en `Endpoints.cs`.

Casi olvidaba la configuración de este proyecto, el cual ahí es donde le decimos al Reverse Proxy a donde redireccionará las llamadas que le lleguen:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "ApiRoute": {
        "ClusterId": "api/cluster",
        "Match": {
          "Path": "api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "api/cluster": {
        "Destinations": {
          "default": {
            "Address": "http://localhost:5178"
          }
        }
      }
    }
  },
  "ApiHost": {
    "Url": "http://localhost:5178"
  }
}
```

### Angular

Para ya casi terminar, necesitamos implementar el frontend con angular, utilizaremos lo que ya nos creó la plantilla.

Creamos un componente llamado `login` y escribimos lo siguiente:

```html
<div class="d-flex justify-content-center align-items-center vh-100 bg-light">
  <div class="container">
    <div class="row justify-content-center">
      <div class="col-sm-8 col-md-6 col-lg-4">
        <form #loginForm="ngForm" (ngSubmit)="onSubmit()" class="p-4 shadow rounded bg-white">
          <h3 class="text-center mb-4">Inicio de sesión</h3>
          <!-- UserName -->
          <div class="mb-3">
            <label for="username" class="form-label">
              <i class="bi bi-person-fill me-2"></i>Username
            </label>
            <div class="input-group">
              <input type="text" class="form-control" id="username" name="username" [(ngModel)]="loginModel.username"
                required>
            </div>
          </div>
          <!-- Password -->
          <div class="mb-3">
            <label for="password" class="form-label">
              <i class="bi bi-lock-fill me-2"></i>Password
            </label>
            <div class="input-group">
              <input type="password" class="form-control" id="password" name="password"
                [(ngModel)]="loginModel.password" required>
            </div>
          </div>
          <div class="d-grid">
            <button type="submit" class="btn btn-primary" [disabled]="isBusy">
              <i class="bi bi-box-arrow-in-right me-2"></i>Login
            </button>
          </div>
        </form>
      </div>
    </div>
  </div
</div>
```

Aquí es un formulario con clases de bootstrap 5, muy default generado por copilot 🤣🥳.

Cuando se envíe el formulario se mandará a llamar el método `onSubmit()` para procesar los datos ingresados.

El botón de "Login" se deshabilitará cuando la variable `isBusy` sea `true`, lo que evita que el usuario haga clic varias veces en el botón mientras se procesa la solicitud de inicio de sesión.

```ts
import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../shared/authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  public isBusy = false;
  public loginModel: {
    username?: string,
    password?: string
  } = {};
  
  constructor(
    private authService: AuthenticationService
  ) { }
  
  ngOnInit(): void {
  }
  
  onSubmit() {
    if (this.isBusy) {
      return;
    }
  
    this.isBusy = true;
  
    // Post loginModel a /login
    this.authService.login(this.loginModel.username!, this.loginModel.password!)
      .subscribe(_ => {
        window.location.href = '/home';
      })
      .add(() => this.isBusy = false);
  }
}
```

Al mandar el formulario se llama al servicio de autenticación para realizar la solicitud de inicio de sesión. Si la solicitud es exitosa, redirige al usuario a la página de inicio.

El servicio `AuthenticationService` tiene los siguientes métodos y funcionalidades:

```ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, of, tap } from 'rxjs';
  
@Injectable({
  providedIn: 'root'
})

export class AuthenticationService {

  constructor(private http: HttpClient) { }
  
  saveUser(user: any) {
    localStorage.setItem('user', JSON.stringify(user));
  }
  
  getUser() {
    return JSON.parse(localStorage.getItem('user')!);
  }

  isAuthenticated() {
    return !!this.getUser();
  }
  
  isInRole(role: string) {
    return this.isAuthenticated() && this.getUser().roles.includes(role);
  }
  
  login(username: string, password: string): Observable<any> {
    return this.http.post('local-login', { username, password })
      .pipe(
        catchError(error => {
          console.error('Error en la solicitud de inicio de sesión:', error);
          throw error;          
        }),
        tap((response: any) => {
          this.saveUser(response);
        })
      );
  }
  
  logout() {
    this.http.post('local-logout', {}).subscribe(result => {
      localStorage.removeItem('user');
      window.location.href = '/login';
    });
  }
}
```

Resumen:
- `isAuthenticated()`: Verifica si el usuario está autenticado comprobando si hay un usuario en el almacenamiento local. Retorna `true` si el usuario está autenticado y `false` en caso contrario.
	- Si alguien intenta crear un usuario manualmente y guardarlo en localStorage, en efecto este servicio lo considerará "autenticado" pero al querer hacer cualquier operación como usuario autenticado no podrá, ya que no tendrá ninguna cookie válida.
- `isInRole(role: string)`: Verifica si el usuario autenticado tiene un rol específico. Comprueba si el usuario está autenticado y si el rol especificado está presente en el array de roles del usuario.
	- También aquí puede suceder que se agreguen roles y se haga escalación de permisos, pero los roles siempre y digo siempre, se deben de revisar en el backend antes de realizar o mostrar información.
- `login(username: string, password: string)`: Realiza una solicitud POST al endpoint `local-login` del BFF con las credenciales de inicio de sesión proporcionadas. Si la solicitud es exitosa, el método guarda el objeto de usuario en el almacenamiento local y devuelve la respuesta del servidor.
- `logout()`: Realiza una solicitud POST al endpoint `local-logout` del BFF para cerrar la sesión del usuario. Después de cerrar la sesión, el método elimina el objeto de usuario del almacenamiento local y redirige al usuario a la página de inicio de sesión.

> Nota 💡: Esta implementación al final viene siendo un ejemplo del concepto, esto puede variar o mas bien, deberías hacerlo a tus necesidades.

#### Protección de rutas

La protección de rutas se implementa utilizando el guard `AuthGuard`. El archivo `auth.guard.ts` contiene la implementación del guard.

El guard `AuthGuard` se utiliza para proteger las rutas de la aplicación y asegurarse de que solo los usuarios autenticados puedan acceder a ellas. Para hacer esto, el guard implementa el método `canActivate()`, que se llama antes de cargar una ruta.

```ts
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationService } from './authentication.service';
  
@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  
  constructor(
    private authService: AuthenticationService,
    private router: Router) {
    
  }
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
  
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
  
    // TODO: Revisar Roles
  
    return true;
  }
  
}
```

En el método `canActivate()`, se verifica si el usuario está autenticado utilizando el servicio `AuthenticationService`. Si el usuario está autenticado, se permite la navegación a la ruta solicitada. Si el usuario no está autenticado, se redirige al usuario a la página de inicio de sesión.

#### Directiva de autorización

La directiva de autorización (`AuthorizeDirective`) se utiliza para mostrar u ocultar elementos del DOM según el rol del usuario autenticado. El archivo `authorize.directive.ts` contiene la implementación de la directiva.

```ts
import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthenticationService } from './authentication.service';
  
@Directive({
  selector: '[appAuthorize]'
})
export class AuthorizeDirective {

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthenticationService
  ) { }
  
  @Input() set appAuthorize(roleName: string) {
    if (!this.authService.isInRole(roleName)) {
      this.viewContainer.clear();
    } else {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }
  
}
```

La directiva tiene un `@Input` llamado `appAuthorize`, que permite especificar el rol necesario para mostrar el elemento. Si el usuario autenticado tiene el rol especificado, se muestra el elemento. De lo contrario, se oculta.

La directiva utiliza el servicio `AuthenticationService` para verificar si el usuario autenticado tiene el rol requerido. Utiliza `ViewContainerRef` para manipular el contenedor de vistas y mostrar u ocultar elementos del DOM.

#### Products

Ahora vamos a crear la vista de productos, que muestra una tabla con una lista de productos protegidos.

Como ejemplo, solo los usuarios con el rol de "Administrator" podrán ver un botón para crear productos.

```html
<h2>
  Products
  <small>(protected resources)</small>
</h2>

<!-- Ejemplo de Authorize -->
<button class="btn btn-primary" *appAuthorize="'Administrator'">Create Product</button>
  
<!-- Tabla con productos -->
<table class="table table-striped">
  <thead>
    <tr>
      <th>Id</th>
      <th>Nombre</th>
    </tr>
  </thead>
  <tbody>
    <tr *ngFor="let product of products">
      <td>{{product.id}}</td>
      <td>{{product.name}}</td>
    </tr>
  </tbody>
</table>
```

En el archivo `products.component.ts`:

```ts
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
  
@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css']
})
export class ProductsComponent {
  public products: any[] = [];
  
  constructor(private httpClient: HttpClient) {
    this.getProducts();
  }
  
  getProducts() {
    this.httpClient.get('/api/products').subscribe((response) => {
      this.products = response as any[];
    });
  }
}
```

En este código, creamos el componente `ProductsComponent` que se encargará de mostrar la lista de productos protegidos. El componente utiliza el servicio `HttpClient` para realizar una solicitud GET al endpoint `/api/products` y obtener la lista de productos.

> Nota 💡: Recuerda que `/api/products` se encuentra en API pero desde Angular lo estamos llamando a su `BFF`. El `BFF` se encargará de hacer la redirección y también ya incluirá el JWT que viene en la Cookie (que es mandada en automático al hacer la solicitud HTTP).

# Conclusión

La implementación del patrón Backend for Frontend utilizando Angular como aplicación cliente y ASP.NET Core junto con YARP como backend ofrece una forma eficiente y segura de gestionar la comunicación entre el frontend y el backend. La combinación de autenticación por cookies en el BFF y autenticación por Bearer Tokens en el backend API garantiza la seguridad de las solicitudes y el acceso a los recursos protegidos. Al utilizar YARP, podemos transformar las peticiones del BFF de manera segura y eficiente antes de enviarlas al backend API. Esta arquitectura proporciona una solución robusta para construir aplicaciones web escalables y seguras.

En resumen, implementar el patrón Backend for Frontend en aplicaciones SPA es crucial para abordar los desafíos de seguridad asociados con el manejo del JWT. Al utilizar el BFF, podemos garantizar un almacenamiento y transmisión seguros de la información de autenticación, mitigando los riesgos de ataques XSS y phishing. Además, el BFF ofrece una separación de responsabilidades y una adaptación a las necesidades del cliente, mejorando así la modularidad y la eficiencia del sistema en general.