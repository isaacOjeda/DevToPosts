# ASP.NET Core: Creación de Cookies Seguras con IDataProtector

Created: Feb 24, 2021 8:52 AM
Tags: draft

Hola de nuevo Devs 👋🏽!

En la entrada de hoy quiero compartirles una funcionalidad muy útil de .NET, que en si no es nueva, ya existía algo similar en .NET Framework pero me gustó la facilidad de usarlo y la infinidad de usos que se le pueden dar.

## IDataProtector

`IDataProtector` nos ofrece una forma muy fácil para proteger información en nuestras aplicaciones.

Cuando queremos guardar información sensible, ya sean contraseñas o tarjetas de crédito es esencial que se utilice un algoritmo de encriptación seguro.

Aquí es donde `IDataProtector` y .NET hacen la magia, ya que esta interfaz nos deslinda de la responsabilidad de pensar en algoritmos seguros o de soportar una retrocompatibilidad cuando hablamos de versiones nuevas de algoritmos.

El uso que yo últimamente le di con un proyecto fue para encriptar información sensible que era persistida en una cookie y el navegador del cliente, pero realmente el uso que se le pueda dar es el que necesites.

## Cookies Seguras

Puedes revisar mi [Github](https://github.com/isaacOjeda/DevToPosts) donde esta todo el código y pues así es más fácil ver toda la solución.

La intención de todo esto es generar una cookie que solo el servidor pueda leer (y modificar) y para esto creamos un servicio llamado `SecureCookiesService`

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SecureCookiesExample.Services
{
    public class SecureCookiesService
    {
        private readonly IDataProtector _dataProtector;
        private readonly HttpContext _http;

        public SecureCookiesService(
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(SecureCookiesService));
            _http = httpContextAccessor.HttpContext;
        }

        public T GetCookieValue<T>(string name)
        {
            if (_http.Request.Cookies.ContainsKey(name))
            {
                var secureJson = _http.Request.Cookies[name];
                var rawJson = _dataProtector.Unprotect(secureJson);

                return JsonSerializer.Deserialize<T>(rawJson);
            }

            return default;
        }

        public void CreateCookie<T>(string name, T data)
        {
            var rawJson = JsonSerializer.Serialize(data);
            var secureJson = _dataProtector.Protect(rawJson);

            _http.Response.Cookies.Append(name, secureJson, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });
        }
    }
}
```

**IDataProtectionProvider** nos ayuda a crear el protector y debemos darle un "identificador" o mejor dicho un "propósito". Para eso usamos `nameof`. Cuando se crea un **IDataProtector** con un **Purpose**, automáticamente se crean las llaves necesarias para que ese protector pueda encriptar y desencriptar, pero si su purpose es diferente, este no podrá desencriptar información de otro protector. 

También se hace uso de `System.Text.Json`para serializar y deserializar objetos en JSON. Así podemos guardar lo que queramos en la cookie de una forma más modelada.

La Cookie que se genera es **HttpOnly** y eso significa que solo el servidor puede escribir en esa cookie. También es **Secure**, por lo tanto solo puede ser manipulada por medio de **HTTPS**.

**Protect** y **Unprotect** hacen la magia, realmente no hay que hacer nada, simplemente pasar el string que se quiere encriptar y viceversa para obtener el valor original.

Y para poderlo usar, tenemos que tener nuestro **Startup.cs** listo de la siguiente manera:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecureCookiesExample.Services;

namespace SecureCookiesExample
{
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddDataProtection();
            services.AddHttpContextAccessor();

            services.AddTransient<SecureCookiesService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
```

Para fines prácticos del ejemplo, solo voy a tener una vista de Razor Pages para mostrar la funcionalidad de la generación de la Cookie llamada **Index.cshtml** y su PageModel **Index.cshtml.cs**:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureCookiesExample.Services;

namespace SecureCookiesExample.Pages
{
    public class IndexPageModel : PageModel
    {
        private readonly SecureCookiesService _secureCookiesService;

        public IndexPageModel(SecureCookiesService secureCookiesService)
        {
            _secureCookiesService = secureCookiesService;
        }

        public void OnGet()
        {
            _secureCookiesService.CreateCookie(".ExampleCookie", new CookieExampleModel
            {
                Name = "Isaac Ojeda",
                UserId = 123456
            });
        }

        public class CookieExampleModel
        {
            public int UserId { get; set; }
            public string Name { get; set; }
        }
    }
}
```

```html
@page
@model IndexPageModel

@using SecureCookiesExample.Services
@inject SecureCookiesService SecureCookies

@{
    var cookieData = SecureCookies.GetCookieValue<IndexPageModel.CookieExampleModel>(".ExampleCookie");
}

<h2>Cookies seguras</h2>
@if (cookieData is not null)
{
    <p><strong>Encriptada:</strong> @Request.Cookies[".ExampleCookie"]</p>
    <p><strong>Desencriptada:</strong> @cookieData.Name @cookieData.UserId</p>
}
else
{
    <p>Refresh again!</p>
}
```

Lo que esta sucediendo en el PageModel es que cada vez que se haga HTTP GET a "/" siempre se guardará una cookie nueva con los mismo valores.

Sn el Razor Page mostramos el valor de la cookie encriptado (como le llega al usuario) y también desencriptado (algo que solo el servidor puede hacer).

Lo interesante aquí es que cada vez que encriptamos los mismos valores, siempre tendremos resultados diferentes, es lo genial de esto. Siempre será mucho más seguro de esta forma.

Ejemplo:

## Cookies seguras

**Encriptada:** CfDJ8MUS1DqDNaJCuzodb8S7vN6uS8k3pf97x90rklUKQMebeq04S8JvaBA_GpQoCel8y53b31kcvo8hHNjIyIfuEQD9CNXc9Gs0Z4tyEaG0k6ALlLup4444n9-eOk3f0AguJ2a6c4BlPRb_HzBTLR4vfbb1J-oTFAMz9jrrGCakUtwx

**Desencriptada:** Isaac Ojeda 123456

---

Como lo comentaba anteriormente, esto tiene infinidad de usos pero mi requerimiento donde surgió la idea de este post, era crear cookies encriptadas (que realmente, es algo suuuuper común).

Te recuerdo de nuevo el link de mi [Github](https://github.com/isaacOjeda/DevToPosts/tree/main/SecureCookiesExample) para que descargues el código y puedas visualizar mejor el ejemplo.

Recuerda: Code4Fun✌