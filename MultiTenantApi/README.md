

# Introducción

En este post veremos nuevamente como hacer una aplicación multi-tenant, pero ahora enfocado a crear una REST API con ASP.NET Core.

Crear aplicaciones multi-tenant se vuelven un reto cuando se comienza en el desarrollo, aunque siempre recomiendo conocer los cimientos de las cosas, en esta ocasión veremos como crear una aplicación multi-tenant utilizando una librería muy útil, **Finbuckle**. 

**Finbuckle** de verdad que facilita el trabajo y básicamente te lo hace todo, así que comenzemos.

Como siempre, [aquí](https://github.com/isaacOjeda/DevToPosts/tree/main/MultiTenantApi) puedes consultar el código para que no batalles en seguir este post.

# Web API Multi-Tenant

Para hacer una aplicación multi-tenant, una de las partes en las que se define el cómo haremos la implementación, es sabiendo ¿Cómo vamos a identificar a los tenants?

> Nota 💡: Ya he hablado en múltiples ocasiones sobre este tema de aplicación multi-tenant. Puedes revisar las series para tener un mejor contexto 
> [ASP.NET Core Multitenancy Series' Articles - DEV Community](https://dev.to/isaacojeda/series/14725)
> [ASP.NET: Authentication Multi-Tenant Series' Articles - DEV Community](https://dev.to/isaacojeda/series/19844)

En post pasado menciono las distintas formas de identificar un tenant de otro, los ejemplos que hemos trabajado siempre son por medio del host, pero las formas comunes son:
32
- Host o subdomain: `https://{my-tenant-name}.balusoft.com`
- Path: `https://api.balusoft.com/{my-tenant-name}`
- Header (el que usaremos): `https://api.balusoft.com` incluyendo un header (con el nombre que quieras), ejem. `X-Tenant: {my-tenant-name}`
- Cookie: Nunca lo he hecho, pero también podría funcionar, tener una cookie donde se establezca el tenant en el que usuario inició sesión.

Cuando hablamos de una API, no es necesario tener un dominio dedicado al tenant, puede ser el mismo dominio (ejem. `https://api.balusoft.com`) y la forma de diferenciar entre un tenant y otro es por algún Header o Path, no el dominio tal cual.

Lo que haremos hoy es eso, un servicio web que será multi-tenant, y cada tenant tendrá su propia base de datos. La forma de identificar cada tenant, será por medio de un Header que los clientes de la API tendrán que mandar y así identificar el tenant. 

> Nota 💡: Una aplicación multi-tenant no está obligada a que tenga una base de datos por tenant. Si revisas los post's mencionados anteriormente, sabrás que puedes tener aplicaciones seguras multi-tenant con una sola base de datos.

Por lo general, cuando hago uso de HTTP Headers "Custom" lo que siempre veo que se hace, es usar un prefijo "X-" para identificar que es un header no-standard. Esto no afecta en nada, pero al menos se busca prevenir no colisionar con encabezados que sí son estándar y los navegadores o servidores web pueden o necesitan usar.

## Proyecto MultiTenantApi

Crearemos un proyecto web vacío para hacer todo desde cero:

`dotnet new web -o MultiTenantApi`

Utilizaremos los siguientes paquetes:
```xml
    <PackageReference Include="Finbuckle.MultiTenant.AspNetCore" Version="6.10.0" />
    <PackageReference Include="Finbuckle.MultiTenant.EntityFrameworkCore" Version="6.10.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.4">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.4" />
```

Utilizamos **Finbuckle** para agregar soporte multi-tenant, es muy flexible y la verdad lo hace demasiado fácil.

> Nota 💡: En post's anteriores hacemos la implementación a mano desde 0 siguiendo un approach muy similar (casi identico) a Finbuckle, si quieres aprender hacerlo tú, te recomiendo la serie Multitenancy

También utilizaremos **EntityFramework Core** simplemente para tener una persistencia que puede ser en memoria, pero sin ningún problema cambiarla a una base de datos real (como aquí que usaremos SQL Server).

## Entities y DbContext

Necesitamos tener dos bases de datos en este approach que seguiremos, una base de datos "maestra" que contendrá toda la información de los tenants, como su cadena de conexión de base de datos y como su identificador (que lo usaremos para diferenciar los tenants).

Y para fines de puro ejemplo, tendremos una base de datos que será la de la aplicación, esta será la base de datos única por tenant, por lo que tendremos que crear varias de estas bases de datos para hacer las pruebas y confirmar que estamos creando una aplicación aislada entre cada tenant.

### Entities > Product
Cómo siempre, siempre hago un catálogo de productos:

```csharp
namespace MultiTenantApi.Entities;
  
public class Product
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public double Price { get; set; }
}
```

### Data > Api

Dentro de Data > Api tendremos el DbContext de la aplicación y sus migraciones:

```csharp
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using MultiTenantApi.Entities;
  
namespace MultiTenantApi.Data.Api;
  
public class ApiDbContext : DbContext
{
    private readonly ITenantInfo? _tenant;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
  
    public ApiDbContext(
        DbContextOptions<ApiDbContext> options,
        IWebHostEnvironment env,
        IMultiTenantContextAccessor multiTenantContextAccessor,
        IConfiguration config)
        : base(options)
    {
        _tenant = multiTenantContextAccessor.MultiTenantContext?.TenantInfo;
        _env = env;
        _config = config;
    }
  
    public DbSet<Product> Products => Set<Product>();
  
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string? connectionString;
  
        if (_tenant is null && _env.IsDevelopment())
        {
            // Init/Dev connection string
            connectionString = _config.GetConnectionString("Default");
        }
        else
        {
            // Tenant connection string
            connectionString = _tenant!.ConnectionString;
        }
  
        optionsBuilder.UseSqlServer(connectionString);
  
        base.OnConfiguring(optionsBuilder);
    }
}
```

Cuando registremos `ApiDbContext` como dependencia, no vamos a especificar su cadena de conexión ni su proveedor de base de datos, esto lo haremos aquí en el `OnConfiguring` porque será dinámico, según el tenant en el que se está accediendo en ese momento, estableceremos la cadena de conexión.

Aquí ocurre algo también, ya que este `DbContext` suele inicializarse cuando hacemos migraciones, por lo que no existirá un `HttpContext`. Si es el caso, significa que estamos en modo desarrollo (seguramente) y estamos haciendo una migración. Por eso cuando ocurre ese caso en particular, establecemos una cadena de conexión Default, que no será de ningún tenant real, solo de desarrollo.

`IMultiTenantAccessor` nos permite acceder a todo lo relevante del tenant actual, por ahora solo nos interesa la cadena de conexión, ya que eso es lo que hace la "magia" de simplemente nosotros utilizar un `DbContext` como siempre lo hacemos, pero aquí será redireccionado a una base de datos según el tenant.

### Data > Tenants

Esta base de datos, como comenté antes, servirá para guardar un registro de todos nuestros tenants, funcionando como una base de datos "maestra" al cual la API accederá para conocer todos los tenants disponibles.

```csharp
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
  
namespace MultiTenantApi.Data.Tenants;

  
public class TenantsDbContext : EFCoreStoreDbContext<TenantInfo>
{
    public TenantsDbContext(DbContextOptions options) : base(options)
    {
    }
}
```

Aquí estamos usando Finbuckle y su pre-implementación de un `DbContext` que contiene el entity `TenantInfo` con la información básica que necesitamos.

> Nota 💡: Sin ningún problema se puede implementar la interfaz `ITenantInfo` para usarla en lugar de `TenantInfo` y agregar las propiedades que se necesiten.

### Program

Ya solo resta conectar todos los cables dentro de **Program** para poder hacer pruebas y primero que nada, crear las bases de datos que utilizaremos en este demo.

```csharp
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
  
// Endpoints Van Aquí
  
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
```

Aquí va la explicación:
- **DbContexts**: Aquí registramos los dos `DbContext`'s que tenemos. `TenantsDbContext` sí se indica su cadena de conexión, ya que esta será la "maestra" y `ApiDbContext` no se establece su cadena de conexión por que esta será dinámica desde el `OnConfiguring` que ya definimos antes.
- **MultiTenancy**: En esta parte agregamos lo que **Finbuckle** ya tiene implementado para nosotros. Agregamos el soporte multi-tenant indicando que `TenantInfo` será la implementación de `ITenantInfo` que usaremos (si quisiéramos usar otro entity, aquí lo indicaríamos). 
	- También indicamos que utilizaremos una estrategia de detección de tenants por medio de un encabezado http, aquí decimos que será el encabezado **X-Tenant**
	- Por último, indicamos el origen de los tenants, el cual será EntityFramework Core (hay distintos origenes de datos, puede ser por appsettings, en memoría o uno custom)
- **SeedTenantData**: Este método lo único que hace es dar de alta dos tenants (tenant01 y tenant02) el cual utilizaremos para el demo y confirmar que esta implementación funciona.
	- Estamos indicando que cada uno tendrá una base de datos diferente, por lo que tendremos que crearlas con Scripts o con `dotnet ef database update`

### Migraciones
Hasta este punto ya podemos hacer las migraciones, como tenemos dos contextos en una misma solución, es un poco diferente el hacerlo. Para `TenantsDbContext` hacemos lo siguiente:

```bash
dotnet ef migrations add FirstMigration -o Data/Tenants/Migrations --context TenantsDbContext
```

El cual creará la carpeta **Tenants > Migrations**.

Y para `ApiDbContext`:

```bash
dotnet ef migrations add FirstMigration -o Data/Api/Migrations --context ApiDbContext
```

Y también, creará su folder **Api > Migrations**. Aquí se utilizará la cadena de conexión **Default** que tengamos en el appsettings, podemos usar esta base de datos para crear otras bases de datos (o podemos generar los scripts con comandos `dotnet-ef`).

En fin yo así tengo mis dos bases de datos de prueba:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/ku12idg2afbuuw30rskk.png)

Y en cada una di de alta manualmente varios productos:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/lqodtnnu0latwoq6std8.png)

### Endpoints

Para hacer nuestras pruebas, haremos dos endpoints (en donde estaba el comentario `// Endpoints Van Aquí`):

```csharp
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
```

Utilizando la extensión `GetMultiTenantContext` (o también utilizando `IMultiTenantContextAccessor`) podemos acceder a la info del tenant actual (determinado por el header `X-Tenant`) y también usamos el DbContext para consultar los productos.

Lo genial aquí es esto, ya todo está configurado para que funcione automáticamente de una forma aislada por tenant, todo lo que hagamos al `ApiDbContext` lo hará según el tenant.

## Probando la solución

Utilizando [Rest Client]([REST Client - Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)) de VS Code podemos hacer las siguientes pruebas

```bash
@host = http://localhost:5087
  
### Tenant 01
GET {{host}}
Content-Type: application/json
X-Tenant: tenant01
```

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/lp9kysh8akrd5l1xfp5z.png)

```bash
### Tenant 01 Products
GET {{host}}/api/products
Content-Type: application/json
X-Tenant: tenant01
```

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/ehg96ce6u5qtpb8i2nhw.png)

```bash
### Tenant 02
GET {{host}}
Content-Type: application/json
X-Tenant: tenant02
```


![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/6jiidr20865f8cr52b30.png)



```bash
### Tenant 02 Products
GET {{host}}/api/products
Content-Type: application/json
X-Tenant: tenant02
```


![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/vlno7m1v5gn8tikzhbqt.png)


# Conclusión 
Hemos creado una REST API multi-tenant con un aislamiento con base de datos, crear tu proyecto partiendo de aquí ya es como si desarrollaras una API como siempre lo haces.

Te recomiendo que visites los post's que menciono aquí, para que comprendas y aprendas más acerca de las distintas opciones que tenemos al desarrollar aplicaciones multi-tenant.

Con todo gusto atenderé tus dudas, sígueme en [@balunatic](https://twitter.com/balunatic) y conectamos.

Te recomiendo que visites la documentación de **Finbuckle**, la verdad tiene mucho más funcionalidad, me gustaría abarcar más, pero será en otra ocasión. 

🖖🏼

# Referencias 

- [Finbuckle.MultiTenant Docs](https://www.finbuckle.com/MultiTenant/Docs/v6.10.0)
- [Finbuckle/Finbuckle.MultiTenant (github.com)](https://github.com/Finbuckle/Finbuckle.MultiTenant)