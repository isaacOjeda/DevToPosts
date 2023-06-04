## Introducción

El post de hoy será un poco largo y tal vez un complicado, pero es un tema muy interesante e importante para las aplicaciones que hoy corren en la nube.

Hoy veremos como configurar toda una suite de APM (Application performance monitoring) utilizando software open source y libre de usar con imágenes de Docker y OpenTelemetry.

Crearemos una aplicación Web API para consultar el clima (clima real, de un api pública gratuita llamada [Free Open-Source Weather API | Open-Meteo.com](https://open-meteo.com/)) y también crearemos dashboards chulos en Grafana que podrán monitorear cualquier aplicación y cualquier instancia (si está escalada) de la aplicación.

Existe mucha documentación, pero aquí trataré de consolidad todo para una aplicación WebAPI utilizando Grafana, Prometheus, Loki, Zipkin y el OTEL Collector de OpenTelemetry.

Que esto me lleva a lo más importante: **OpenTelemetry**.

Este ya es un estándar de telemetría y aunque aún es nuevo, muchos servicios lo están adoptando. Ya que hay servicios como Application Insights de Azure o Datadog que ya te permiten recibir telemetría de aplicaciones por medio del protocolo que OpenTelemetry creó, el OTLP.

> Nota 💡: Para seguir este post es necesario tener docker instalado y actualizado

> Nota 2 💡: El código fuente lo encuentras siempre en mi [repositorio de github](https://github.com/isaacOjeda/DevToPosts/tree/main/OpenTelemetryExample)

## APM (Application Performance Monitoring)
Ya hemos hablado en este blog sobre monitoreo de [aplicaciones utilizando application insights](https://dev.to/isaacojeda/parte-11-aspnet-core-application-insights-y-serilog-3103), lo cual es cierto que ya es una solución completa y es la que yo uso todos los días para monitorear los proyectos en los que participo. Pero no todos usan Azure, así que hoy veremos otras opciones para poder implementar un APM.

Las ventajas de tener un monitoreo eficiente, es poder detectar anomalías y saber responder a ellas. La idea siempre será mejorar el software para que la experiencia de nuestros usuarios sea la mejor.

Tener la mayor visibilidad de cómo opera nuestro sistema nos da una gran ventaja y es algo que todos deberíamos de tener en nuestros proyectos y hoy veremos cómo hacerlo con puro Open Source (todos proyectos de la Cloud Native Foundation).

Lo que debe de tener un APM es lo siguiente:

**Logs**
Siempre nuestras aplicaciones deben de generar logs, ya sean de error o informativos. Estos siempre nos ayudarán a entender en donde falla nuestra aplicación y será más fácil diagnosticar los problemas.

En desarrollo por lo general todos los logs están activados, pero en producción lo más normal es solo mostrar los Warnings/Criticals:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/0b3pnkefdh56oa1qfm42.png)
_Ejemplo de logs con Loki y Grafana_

Poder consultar los logs sí o sí nos ayudarán a darnos cuenta de muchos problemas que nuestra aplicación pudiera tener y los desarrolladores podrán solucionar los bugs sin que batallen tanto. Porque por ejemplo, los `StackTrace` y `Exceptions` que ocurren, siempre se loggean y sin problema los podemos consultar para saber dónde falló la aplicación. 

**Tracing**
Un trace es un grupo de operaciones o transacciones con [spans](https://www.elastic.co/guide/en/apm/guide/current/data-model-spans.html) que fueron originados por un HTTP Request.

Es muy común que las aplicaciones web se comuniquen con otros servicios o bases de datos, por lo que, un trace completo nos ayudaría a seguir toda esa línea de ejecución sin importar que esta se encuentre distribuida en distintos servicios y fácilmente poder saber dónde hay problemas.

Un ejemplo:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/q633vkhrilz2qwcdf53v.png)
_Ejemplo de Tracing de APM de Elastic Observability_

Aquí vemos que el request inicial fue `GET /dashboard` y cada servicio generó sus propios spans, pero de un mismo trace original. Aquí fácilmente podemos localizar en donde falló un request o que es lo que está lento cuando hay algo mal en algún servicio.

**Metrics**
Las métricas también son parte muy importante, estas nos ayudarán conocer muchas cosas sobre nuestra aplicación, como: Uso de CPU, memoria disponible, solicitudes fallidas, tiempo de respuesta por solicitud, etc.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/p8n1kuqf90qa9c10mbci.png)
_Ejemplo de Metrics de APM de Elastic Observability_

### Infraestructura
Las tecnologías que utilizaremos son las siguientes:
- **Prometheus**
	- Este será nuestra base de datos y el que recolectará toda la información para las **métricas**
- **Loki**
	- Es una base de datos dedicado a los **logs**, este recibirá y guardará los logs de una manera que se puedan obtener después
		- Otro servicio de logs muy bueno es [Seq](https://datalust.co/seq) de datalust.co, pero por ahora no tiene soporte para OpenTelemetry
- **Zipkin**
	- Nos permitirá guardar todos los `traces` y `spans` que la aplicación (o aplicaciones) generen
		- Otro servicio open source para tracing es [Jaeger](https://www.jaegertracing.io/) que sí soporta OpenTelemetry
- **Grafana**
	- Grafana nos ayudará a mostrar toda la información recolectada por los anteriores, aquí podemos crear dashboards y explorar toda la información (aunque zipkin tiene su propia UI y Prometheus también).
	- Los dashboards van a incluir gráficas similares a las capturas mostradas anteriormente y es totalmente libre de configurar a las necesidades de cada uno.
- **OpenTelemetry Collector**: 
	- Este es opcional, pero es recomendado. El OTEL Collector es un intermediario entre mi aplicación y todos los servicios anteriores que hemos mencionado, nuestra aplicación no sabrá a donde va toda la información, solo le importa que se va a comunicar con un protocolo especial de OpenTelemetry y el OTEL Collector se encargará de mandarlo a cada servicio según se requiera.
	- Esto es muy útil cuando tenemos muchos servicios, solo configuramos el OTEL (autenticación, certificados, cosas tediosas) pero las aplicaciones de igual forma solo mandan su información al collector sin preocuparse de los detalles.
	- También es útil cuando quisiéramos cambiar de proveedor de APM, nuestra aplicación no se enterará de nada, solo actualizamos el Collector y ya.
		- Muchos APMs ya soportan OpenTelemetry por que ya es un estándar, por lo que es una muy buena ventaja sí utilizar este collector.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/ca59ygk9o2dgtdjzqwcd.png)

Lo que haremos a continuación es lo siguiente:

- Tendremos una Web API que se comunicará con una API Pública del Clima.
- La Web API mandará los Logs, Traces y Metrics por medio del protocolo OTLP de OpenTelemetry y se los mandará al OTEL Collector.
- El OTEL Collector los procesará y los mandará a sus exporters (Loki, Prometheus y Zipkin).
- Configuraremos Grafana para visualizar dashboards con las métricas y logs
- Usaremos Zipkin para ver todos los traces

Comencemos.

#### Docker Compose

Lo primero que necesitamos es configurar la infraestructura, no veremos tanto los detalles de la aplicación Web API pero siempre puedes ver el [código fuente](https://github.com/isaacOjeda/DevToPosts/tree/main/OpenTelemetryExample) para basarte en ello.

```yaml
version: "3"
  
services:
  loki:
    image: grafana/loki:latest
    ports:
      - "3100:3100"
    command: -config.file=/etc/loki/local-config.yaml
    networks:
      - opentelemetry
  
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    command: --config.file=/etc/prometheus/prometheus.yaml --storage.tsdb.path=/prometheus
    volumes:
      - ./prometheus.yaml:/etc/prometheus/prometheus.yaml
      - ./tmp/prometheus:/prometheus
    networks:
      - opentelemetry
  
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_AUTH_ANONYMOUS_ENABLED=true
      - GF_AUTH_ANONYMOUS_ORG_ROLE=Admin
      - GF_AUTH_DISABLE_LOGIN_FORM=true
    volumes:
      - ./tmp/grafana/:/var/lib/grafana/    
      - ./grafana-datasource.yaml:/etc/grafana/provisioning/datasources/ds.yaml
    depends_on:
      - prometheus
      - loki
    networks:
      - opentelemetry
  
  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    ports:
      - "4317:4317"
      - "4318:4318"
    command: --config=/etc/otel-collector-config.yaml
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    networks:
      - opentelemetry
  
  zipkin:
    image: openzipkin/zipkin:latest
    ports:
      - "9411:9411"
    networks:
      - opentelemetry
  
networks:
  opentelemetry

    name: opentelemetry-network
```

¿Qué significa todo eso?

Si nunca has usado docker y archivos compose tal vez esto te resultará complicado de procesar, pero la esencia de todo esto es configurar cada servicio según sus imagenes docker que ya existen publicadas por sus desarrolladores.

Estamos indicando que configuración usar y que puertos usar (por lo general, los defaults) y todos se encuentran en una misma red (por lo cual, sus nombres de host serán los que están indicados aquí, ejemplo: http://loki:3100.

##### grafana-datasource.yaml

```yaml
apiVersion: 1
  
datasources:
- name: Prometheus
  type: prometheus
  access: proxy
  orgId: 1
  url: http://prometheus:9090
  basicAuth: false
  isDefault: true
  version: 1
  editable: false
  
- name: Loki
  type: loki
  access: proxy
  orgId: 1
  url: http://loki:3100
  basicAuth: false
  isDefault: false
  version: 1
  editable: false
  
- name: Zipkin
  type: zipkin
  access: proxy
  orgId: 1
  url: http://zipkin:9411
  basicAuth: false
  isDefault: false
  version: 1
  editable: false
```

Aquí estamos diciendo que Grafana tendrá tres Data Sources (Prometheus, Loki y Zipkin) indicando también el host de donde se obtendrá su información.

Aquí todo es super básico, nada de autenticación y cosas por el estilo, pero todo eso puede configurar si profundizamos en la documentación de cada proveedor.

##### loki-config.yaml

```yaml
auth_enabled: false
  
server:
  http_listen_address: 0.0.0.0
  grpc_listen_address: 0.0.0.0
  http_listen_port: 3100
  
schema_config:
  configs:
    - from: 2020-04-15
      store: boltdb
      object_store: filesystem
      schema: v11
      index:
        prefix: index_
        period: 168h
```

Aquí nuevamente nos vamos por la configuración más básica, Loki puede guardar los logs en muchos lugares, como Storage Blobs de Azure o en Amazon S3.

En este caso, solo los guardamos como archivos por simplicidad.

#####  prometheus.yaml

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  
scrape_configs:
  - job_name: 'otelcollector'
    static_configs:
      - targets: [ 'otel-collector:8889' ]
```

La forma en la que funciona prometheus es haciendo polling de las métricas, por lo que aquí le estamos diciendo de donde conseguirá las métricas y cada cuando debe preguntar por ellas.

Aquí preguntará las métricas al OTEL Collector y lo hará cada 15 segundos. Si no usáramos el collector, aquí irían nuestros proyectos que queremos monitorear, pero aquí la ventaja es que no importa si tenemos 1 o 100 proyectos, siempre se le preguntarán al collector y el nos dará el de todos.

#####  otel-collector-config.yaml

```yaml
receivers:
  otlp:
    protocols:
      http:
      grpc:
  
processors:
  attributes:
    actions:
      - action: insert
        key: loki.attribute.labels
        value: event.domain
  resource:
    attributes:
      - action: insert
        key: loki.resource.labels
        value: service.name
  
  batch:
    timeout: 1s
    send_batch_size: 1024
  
exporters:
  prometheus:
    endpoint: "0.0.0.0:8889"
    send_timestamps: true
    resource_to_telemetry_conversion:
      enabled: true
  
  loki:
    endpoint: http://loki:3100/loki/api/v1/push
    tls:
      insecure: true
  
  zipkin:
    endpoint: http://zipkin:9411/api/v2/spans
  
service:
  pipelines:
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheus]
  
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [zipkin]
  
    logs:
      receivers: [otlp]
      processors: [batch, resource]
      exporters: [loki]
```

Por último, queda la configuración del collector, aquí se definen 4 cosas:
- Los Receivers (los que mandan la información)
	- Nuestra aplicación mandará los datos por medio de OTLP
- Los Processors (de qué forma se procesará la información)
	- Se procesará la info por lotes de 1024 registros (configuración default que Copilot me sugirió 🤭)
- Los Exporters (A donde irá la información procesada)
	- Se expondrán endpoints para el scrapping de prometheus
	- Se mandarán los logs a Loki
	- Se mandarán los Traces a zipkin
- Los pipelines (juntar los 3 anteriores)

Técnicamente a este punto ya podemos correr el docker compose y ver si tenemos algún problema con la configuración:

```batch
docker compose up
```

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/w311cw1f9x63vogumxzy.png)

Y en docker desktop deberíamos de ver todos los servicios corriendo:

![[Pasted image 20230603081926.png]]

> Nota 💡: Siempre puedes descargar el proyecto completo [aquí](https://github.com/isaacOjeda/DevToPosts/tree/main/OpenTelemetryExample) para no batallar tanto si lo quisiste escribir a mano

### Aplicación ASP.NET

Ahora haremos la aplicación API que queremos monitorear, es sencilla ya que solo haremos un endpoint para consultar el clima según coordenadas geográficas utilizando Open Meteo, una API gratuita del clima.

#### NuGets a usar

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="7.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />

<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.5.0-rc.1" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.5.0-rc.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.0.0-rc9.14" />
<PackageReference Include="OpenTelemetry.Instrumentation.EventCounters" Version="1.0.0-alpha.2" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.1.0-rc.2" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.0.0-rc9.14" />
<PackageReference Include="OpenTelemetry.Instrumentation.Process" Version="0.5.0-beta.2" />
  
<PackageReference Include="Serilog.AspNetCore" Version="7.0.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="4.1.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.2.0" />
<PackageReference Include="Serilog.Sinks.OpenTelemetry" Version="1.0.0-dev-00208" />
```

Lo importante:
- Toda la instrumentación de **OpenTelemetry** la vemos en estos paquetes
- OpenTelemetry SDK no soporta por ahora los logs, pero lo hacemos por medio de Serilog y su Sink de OpenTelemetry, es por eso que optamos por Serilog para mandar los logs al Collector

#### Instrumentor

Esta parte es opcional, pero es muy útil si queremos agregar métricas personalizadas, y puede ser de lo que sea, de lo que necesites. Lo hacemos creando un `Instrumentor`, que posteriormente registraremos y automáticamente se mandará al collector para ser consultado y hasta poder hacer gráficas.

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;
  
namespace WebApi;
  
public sealed class Instrumentor : IDisposable
{
    public const string ServiceName = "WebApi";
    public ActivitySource Tracer { get; }
    public Meter Recorder { get; }
    public Counter<long> IncomingRequestCounter { get; }
  
    public Instrumentor()
    {
        var version = typeof(Instrumentor).Assembly.GetName().Version?.ToString();
        Tracer = new ActivitySource(ServiceName, version);
        Recorder = new Meter(ServiceName, version);
        IncomingRequestCounter = Recorder.CreateCounter<long>("app.incoming.requests",
            description: "The number of incoming requests to the backend API");
    }
  
    public void Dispose()
    {
        Tracer.Dispose();
        Recorder.Dispose();
    }
}
```

Al counter (o métrica) le estamos dando el nombre de **app.incoming.requests** y en prometheus estará disponible bajo el nombre `app_incoming_requests_total`. 

#### Configuración de Logs

Como lo mencioné anteriormente, aquí haremos uso de Serilog y el Sink que tiene para OpenTelemetry para mandar todos los logs recoletados por `ILogger`, aquí podemos configurar el nivel del log que queremos que se mande o si queremos que se mande solo en producción y cosas así.

```csharp
const string outputTemplate =
    "[{Level:w}]: {Timestamp:dd-MM-yyyy:HH:mm:ss} {MachineName} {EnvironmentName} {SourceContext} {Message}{NewLine}{Exception}";
  
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.OpenTelemetry(opts =>
    {
        opts.ResourceAttributes = new Dictionary<string, object>
        {
            ["app"] = "webapi",
            ["runtime"] = "dotnet",
            ["service.name"] = "WebApi"
        };
    })
    .CreateLogger();
  
builder.Host.UseSerilog();
```

No hay que olvidar poner `app.UseSerilogRequestLogging();` en los middlewares porque al usar Serilog se elimina cualquier Log que .NET te configura por default, queremos loggear todo lo que sucede en la aplicación, al menos en modo desarrollo.

#### Configuración de Tracing y Métricas

```csharp
builder.Services.AddSingleton<Instrumentor>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(Instrumentor.ServiceName)
        .ConfigureResource(resource => resource
            .AddService(Instrumentor.ServiceName))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(Instrumentor.ServiceName)
        .ConfigureResource(resource => resource
            .AddService(Instrumentor.ServiceName))
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddProcessInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEventCountersInstrumentation(c =>
            {
                // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                c.AddEventSources(
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft-AspNetCore-Server-Kestrel",
                    "System.Net.Http",
                    "System.Net.Sockets");
            })
        .AddOtlpExporter());
```

Aquí ya terminamos de configurar OpenTelemetry indicando que queremos incluir en el tracing y metrics. Todo esto está disponible gracias a todos los paquetes de instrumentación que agregamos y todo se hace ya en automático.

Me puse a jugar con los EventSources y ver que incluye cada uno, pero igual los counters disponibles se encuentran en el link que puse ahí como comentario. 

Aunque creo, que estos que incluimos aquí, son más que suficientes.

#### /api/weather-forecast endpoint

Aquí por fin el endpoint que vamos a monitorear, en esta ocasión consumiremos una API pública del clima para consultar su información según las coordenadas que queramos

```csharp
app.MapGet("/api/weather-forecast", async ([AsParameters] WeatherForeCastParams request) =>
{
    request.Instrumentor.IncomingRequestCounter.Add(1,
        new KeyValuePair<string, object?>("operation", "GetWeatherForecast"),
        new KeyValuePair<string, object?>("minimal-api-route", "/api/weather-forecast"));
  
    var url = $"https://api.open-meteo.com/v1/forecast?latitude={request.Latitude}&longitude={request.Longitude}&hourly=temperature_2m";
  
    var response = await request.HttpClient.GetAsync(url);
  
    response.EnsureSuccessStatusCode();
  
    return await response.Content.ReadFromJsonAsync<WeatherForecast>();
});
```

Struct con los parámetros del endpoint:

```csharp
public struct WeatherForeCastParams
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public HttpClient HttpClient { get; set; }
    public Instrumentor Instrumentor { get; set; }
}
```

Y el DTO que maneja la API del clima:

```csharp
namespace WebApi.Models;
  
public class WeatherForecast
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double generationtime_ms { get; set; }
    public int utc_offset_seconds { get; set; }
    public string timezone { get; set; }
    public string timezone_abbreviation { get; set; }
    public double elevation { get; set; }
    public HourlyUnits hourly_units { get; set; }
    public Hourly hourly { get; set; }
}
  
public class Hourly
{
    public List<string> time { get; set; }
    public List<double> temperature_2m { get; set; }
}
  
public class HourlyUnits
{
    public string time { get; set; }
    public string temperature_2m { get; set; }
}
```

Hasta este punto ya podemos correr la aplicación y deberíamos de poder ya generar métricas (por que en teoria, el collector y toda la infraestructura ya está corriendo).

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/3hodkov182f7eu0pxb3b.png)

### Dashboards en Grafana

Preparé dos dashboards basados obviamente en otros que son públicos y libres de usar, pero estos no usaban el collector, por lo que el nombre de las métricas cambia un poco.

Antes de empezar con los dashboards veamos los datos que ya se empezaron a generar

#### Prometheus
Entrando a http://localhost:9090/ podemos explorar todas las métricas que se están exportando:

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/hnab22urrqj1hntmcaht.png)

#### Zipkin
Entrando a http://localhost:9411/ podemos ya ver los Traces generados

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/yz56ojfu40o9cbfnllvu.png)

Y el detalle de cada uno

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/iukeh8zwp6efbttq2j6b.png)

Aquí si ocurriera algún error, lo veríamos sin problema:
![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/l0ebtgyewayhpez5wnpy.png)

#### Loki
Loki no tiene interfaz tal cual, pero podemos explorar los logs si ya entramos a Grafana, porque este ya tiene todos los datasources configurados.

Entramos a http://localhost:3000/explore y en data source elegimos **Loki**

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/cpa339r0456udft04h8t.png)

Aquí podemos hacer muchas cosas, como transformar el JSON a un texto más legible

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/u3i0umrnqqar1bqliv7k.png)

#### Importar Dashboards

Los dashboards se encuentran dentro de la carpeta `resources` del repositorio (igual aquí está el [01](https://github.com/isaacOjeda/DevToPosts/blob/main/OpenTelemetryExample/resources/grafana-dashboard-01.json) y [02](https://github.com/isaacOjeda/DevToPosts/blob/main/OpenTelemetryExample/resources/grafana-dashboard-02.json)).

Para importarlos solo hay que irnos al menú de Grafana y en dashboards podremos ver la opción *Import* y pegamos el contenido del JSON

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/hm0aho6k8q07ixx59jzv.png)

Si todo sale bien, podremos ver los dashboards con toda la información de WebApi que ya ha estado generanto (por que la dejamos prendida y yo hice varios requests)

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/6k6mfrk5boonr3a13vr8.png)


![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/i1rtcudwxsdr9qyxcss2.png)


## Conclusión

Esto puede resultar abrumador, pero como siempre cuando entras en una tecnología nueva tienes que ser paciente, leer mucho e investigar siempre que tengas dudas.

Aquí podemos ver un walkthrough de como configurar todo de inicio a fin, tener códigos de ejemplo siempre me ayudan a saber como configurar todo.

Por otra parte, aprender a usar el collector resultará útil para todas nuestras aplicaciones ya que omitiremos detalles de los servicios que usaremos para telemetría, estos servicios deberán de contar con autenticación y configuración adicional que con el tiempo puede variar y tener que actualizarlo solo en el collector y no en todos nuestros servicios es la super ventaja que ofrece el OTEL Collector.

Espero te haya servido este post, ya que yo aprendí mucho sobre estas tecnologías y espero que tú también.

## Referencias
- [Guide to OpenTelemetry (logz.io)](https://logz.io/learn/opentelemetry-guide/?utm_source=substack&utm_medium=email)
- [Instrumenting C# .Net Apps With OpenTelemetry | Logz.io](https://logz.io/blog/csharp-dotnet-opentelemetry-instrumentation/#addt)
- [bradygaster/dotnet-cloud-native-build-2023 (github.com)](https://github.com/bradygaster/dotnet-cloud-native-build-2023)
- [cecilphillip/grafana-otel-dotnet: Sample setup showing ASP.NET Core observability with Prometheus, Loki, Grafana, Opentelemetry Collector (github.com)](https://github.com/cecilphillip/grafana-otel-dotnet)
- [open-telemetry/opentelemetry-dotnet: The OpenTelemetry .NET Client (github.com)](https://github.com/open-telemetry/opentelemetry-dotnet)
- [.NET | OpenTelemetry](https://opentelemetry.io/docs/instrumentation/net/)
