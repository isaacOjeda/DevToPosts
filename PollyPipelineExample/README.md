# Polly v8: cómo manejar fallos transitorios con ResiliencePipelineBuilder

## Introducción

Hay una idea que tarde o temprano aparece en cualquier sistema distribuido: no todo error significa que algo esté roto de verdad.

A veces una API tarda más de lo normal. A veces un servicio está reiniciándose. A veces la red falla durante un instante. Y a veces simplemente pegaste justo en el peor momento posible.

Ese tipo de errores existen, pasan seguido y, lo más importante, muchas veces **se resuelven solos si reintentas correctamente**.

Ahí entra Polly.

Con Polly v8 cambió bastante la forma de definir resiliencia en .NET. La API ahora gira alrededor de `ResiliencePipeline` y `ResiliencePipelineBuilder`, que permiten componer estrategias como retry, timeout o circuit breaker de una forma mucho más clara que en versiones anteriores.

Si ya vienes usando `Microsoft.Extensions.Http.Resilience` con `HttpClient`, probablemente ya viste una parte de esta idea. Pero Polly no sirve solo para HTTP. También aplica para operaciones como:

- subir archivos a Blob Storage
- consultar una base de datos
- invocar un servicio gRPC
- procesar mensajes de una cola
- ejecutar cualquier operación async que pueda fallar por causas temporales

En este artículo vamos a usar un ejemplo simple para mostrar cómo aplicar `ResiliencePipelineBuilder`, pero también quiero aprovechar para cubrir mejor el concepto detrás de todo esto: **Retry Pattern**, **transient fault handling** y algunos criterios prácticos para no caer en reintentos ciegos.

## El problema real

Cuando estamos empezando, es común tratar todos los errores igual: si falló, falló. Se lanza la excepción, se loguea algo y listo.

El problema es que en producción eso suele ser demasiado ingenuo.

Imagina este flujo:

```text
Usuario -> Tu app -> Servicio externo -> falla momentáneamente -> error
```

Ahora imagina este otro:

```text
Usuario -> Tu app -> Servicio externo -> falla momentáneamente -> retry -> éxito
```

La diferencia entre ambos no es menor. En el primer caso devuelves un error por algo que tal vez duró 300 milisegundos. En el segundo, absorbiste una falla esperable del entorno y la operación terminó bien.

Eso es resiliencia: no asumir que el mundo es estable, sino diseñar para que el sistema siga funcionando razonablemente bien cuando aparecen fallos normales.

## ¿Qué es transient fault handling?

`Transient fault handling` es la práctica de detectar errores temporales y responder de forma inteligente, en lugar de tratar esos errores como fallos definitivos.

No significa "reintentar todo".

Significa:

1. identificar qué errores son temporales
2. decidir si vale la pena reintentar
3. espaciar esos intentos de forma razonable
4. cortar cuando ya no tiene sentido seguir intentando

Ejemplos comunes de fallos transitorios:

- `TimeoutException`
- errores de transporte representados por `HttpRequestException`
- respuestas `429 Too Many Requests`
- respuestas `408 Request Timeout`
- errores `5xx`
- problemas breves de conectividad
- servicios que están arrancando o recuperándose

Ejemplos de fallos que normalmente **no** conviene reintentar:

- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found` cuando el recurso realmente no existe
- validaciones de negocio fallidas
- errores de formato o datos inválidos

Esta distinción es la base del Retry Pattern. Si no la haces bien, el retry deja de ser una estrategia de resiliencia y pasa a ser una forma elegante de insistir inútilmente.

## En HTTP no basta con capturar `HttpRequestException`

Aquí hay un matiz importante. En operaciones HTTP, no siempre decides reintentar por la excepción que recibes. Muy a menudo lo que realmente importa es el `status code` de la respuesta.

`HttpRequestException` suele representar errores de transporte o conectividad:

- fallo DNS
- conexión rechazada
- corte de red
- handshake TLS fallido
- socket cerrado de forma inesperada

Eso sí suele ser un buen candidato para retry.

Pero en HTTP también existe otro caso: la petición llega al servidor, el servidor responde, y aun así la respuesta indica que conviene reintentar. Ahí el criterio ya no está en la excepción, sino en el código de estado.

En otras palabras:

- si no hay respuesta, normalmente evalúas la excepción
- si sí hay respuesta, normalmente evalúas el `status code`

Además, con `HttpClient` una respuesta `404`, `429` o `503` no lanza excepción por sí sola. Solo obtienes una excepción si tú llamas a `EnsureSuccessStatusCode()` o si el fallo ocurre antes de recibir la respuesta. Por eso, cuando haces retry en HTTP, muchas veces necesitas manejar **ambas cosas**: excepciones de transporte y resultados HTTP no exitosos.

## ¿Cuándo tiene sentido reintentar según el status code?

Como regla general, tiene sentido considerar retry en estos códigos:

- `408 Request Timeout`: el servidor no completó la petición a tiempo
- `429 Too Many Requests`: te están limitando; idealmente debes respetar `Retry-After`
- `500 Internal Server Error`: puede ser transitorio, aunque depende del sistema
- `502 Bad Gateway`: fallo temporal de gateway o upstream
- `503 Service Unavailable`: servicio saturado, caído o en mantenimiento
- `504 Gateway Timeout`: el gateway no recibió respuesta a tiempo del upstream

Hay otros códigos que **a veces** pueden ser reintentables, pero dependen mucho del dominio:

- `409 Conflict`: puede tener sentido si el conflicto es temporal o si hay concurrencia optimista
- `423 Locked`: puede ser temporal si el recurso está bloqueado brevemente
- `425 Too Early`: en algunos escenarios conviene reintentar más tarde

En cambio, normalmente **no** tiene sentido reintentar estos códigos:

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found` cuando el recurso realmente no existe
- `405 Method Not Allowed`
- `422 Unprocessable Entity`

La idea de fondo es simple: si el problema está en tu request, reintentar no arregla nada. Si el problema está en el servidor, en la red o en una condición temporal del entorno, entonces retry sí puede tener sentido.

## Retry Pattern

El Retry Pattern consiste en volver a ejecutar una operación que falló, asumiendo que el error pudo haber sido temporal.

La clave no está en el hecho de reintentar. La clave está en **cómo** reintentas.

Un retry bien hecho:

- solo reintenta errores transitorios
- pone un límite claro de intentos
- espera entre intentos
- idealmente usa backoff y jitter
- deja trazabilidad con logs o métricas

Un retry mal hecho:

- reintenta cualquier excepción
- dispara intentos de forma inmediata
- multiplica la carga sobre un servicio ya degradado
- empeora la latencia total
- puede generar operaciones duplicadas si no hay idempotencia

Si te quedas con una sola idea del artículo, que sea esta: **retry no es un parche; es una decisión de diseño**.

## Un punto importante: idempotencia

Antes de agregar reintentos a cualquier operación, hay una pregunta que conviene hacer siempre:

> Si esta operación se ejecuta dos veces, ¿el resultado sigue siendo correcto?

Eso es idempotencia.

Por ejemplo:

- consultar datos suele ser seguro para reintentar
- actualizar un estado a un valor fijo puede ser seguro
- cobrar una tarjeta dos veces claramente no es seguro

Si una operación no es idempotente, agregar retry sin más puede introducir errores peores que el fallo original.

No quiere decir que no puedas reintentar nunca, pero sí que probablemente necesites una estrategia adicional: claves idempotentes, deduplicación, control transaccional o algún mecanismo parecido.

## Instalación

```bash
dotnet add package Polly
```

## El servicio inestable del ejemplo

Para mostrar el comportamiento de Polly, el proyecto usa un servicio que falla de forma aleatoria. No es sofisticado, pero alcanza para representar bastante bien lo que pasa cuando dependes de infraestructura o servicios externos.

```csharp
public class UnreliableService
{
    private readonly Random _random = new();
    private int _attemptCount = 0;

    public double FailureRate { get; set; } = 0.7;

    public async Task<string> ProcessDataAsync(string data, CancellationToken cancellationToken = default)
    {
        _attemptCount++;
        var currentAttempt = _attemptCount;

        Console.WriteLine($"  [Intento {currentAttempt}] Procesando: \"{data}\"...");

        await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(100, 500)), cancellationToken);

        if (_random.NextDouble() < FailureRate)
        {
            var exception = GetRandomException();
            Console.WriteLine($"  [Intento {currentAttempt}] ❌ Falló con: {exception.GetType().Name}");
            throw exception;
        }

        Console.WriteLine($"  [Intento {currentAttempt}] ✅ Éxito!");
        return $"Procesado: {data} (intento #{currentAttempt})";
    }
}
```

Las excepciones simuladas son justamente de las que suelen entrar en la categoría de fallos transitorios:

- `HttpRequestException`
- `TimeoutException`
- `IOException`
- `InvalidOperationException` con mensaje temporal

En este ejemplo usamos excepciones porque no estamos simulando respuestas HTTP reales. Pero si esta misma idea se aplicara a `HttpClient`, además de las excepciones convendría evaluar el `status code` devuelto por el servidor.

## Creando el pipeline de retry

Aquí es donde Polly v8 muestra su enfoque. En este caso el ejemplo es genérico y por eso decide reintentar según excepciones:

```csharp
using Polly;
using Polly.Retry;

var retryPipeline = new ResiliencePipelineBuilder<string>()
    .AddRetry(new RetryStrategyOptions<string>
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(1),
        UseJitter = true,
        ShouldHandle = new PredicateBuilder<string>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
            .Handle<IOException>()
            .Handle<InvalidOperationException>(ex =>
                ex.Message.Contains("temporarily unavailable")),
        OnRetry = args =>
        {
            Console.WriteLine($"  ⏳ Reintento #{args.AttemptNumber} en {args.RetryDelay.TotalSeconds:F1}s");
            return ValueTask.CompletedTask;
        }
    })
    .Build();
```

## Qué hace cada opción

| Propiedad | Qué significa en la práctica |
|-----------|------------------------------|
| `MaxRetryAttempts` | Cuántas veces más vas a intentar después del primer fallo |
| `BackoffType` | Cómo crece la espera entre reintentos |
| `Delay` | El delay base |
| `UseJitter` | Agrega un pequeño factor aleatorio para que todos los clientes no reintenten al mismo tiempo |
| `ShouldHandle` | Define exactamente qué errores o resultados sí merecen retry |
| `OnRetry` | Te permite registrar eventos, medir, agregar observabilidad o contexto |

## ¿Por qué usar backoff exponencial?

Porque reintentar demasiado rápido suele ser una mala idea.

Si un servicio está saturado o recién se está recuperando, pegarle tres veces seguidas en 50 milisegundos no ayuda. Lo más probable es que solo agraves el problema.

Con backoff exponencial, el intervalo entre intentos crece progresivamente:

```text
Exponential (base 1s): 1s -> 2s -> 4s -> 8s
Linear (base 1s):      1s -> 2s -> 3s -> 4s
Constant (base 1s):    1s -> 1s -> 1s -> 1s
```

En la mayoría de integraciones con servicios externos, exponencial suele ser el punto de partida razonable.

## ¿Y el jitter para qué sirve?

Supongamos que tienes diez instancias de tu app. Todas llaman al mismo servicio. Todas fallan al mismo tiempo. Todas reintentan exactamente un segundo después.

Acabas de crear una mini estampida coordinada.

Eso se conoce como `thundering herd`. El `jitter` rompe ese patrón agregando una pequeña variación aleatoria en el delay. No parece gran cosa, pero en escenarios con volumen real hace bastante diferencia.

## Ejecutando la operación con resiliencia

Una vez construido el pipeline, ejecutas la operación dentro de él:

```csharp
var result = await retryPipeline.ExecuteAsync(
    async ct => await unreliableService.ProcessDataAsync("Datos importantes", ct),
    CancellationToken.None);

Console.WriteLine($"Resultado: {result}");
```

Una salida típica puede verse así:

```text
[Intento 1] Procesando: "Datos importantes"...
[Intento 1] ❌ Falló con: HttpRequestException
⏳ Reintento #1 en 1.2s
[Intento 2] Procesando: "Datos importantes"...
[Intento 2] ❌ Falló con: TimeoutException
⏳ Reintento #2 en 2.1s
[Intento 3] Procesando: "Datos importantes"...
[Intento 3] ✅ Éxito!

Resultado: Procesado: Datos importantes (intento #3)
```

Lo interesante no es solo que eventualmente funcione. Lo importante es que el código de negocio queda limpio, y la política de resiliencia queda definida en un lugar explícito.

## Centralizando pipelines reutilizables

Si vas a usar la misma idea en varios lugares, tiene sentido encapsular la configuración en un factory. El proyecto ya hace algo de eso en `ResiliencePipelines`:

```csharp
public static class ResiliencePipelines
{
    public static ResiliencePipeline<T> CreateRetryPipeline<T>(int maxRetries = 3)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = maxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>()
            })
            .Build();
    }
}
```

Esto tiene varias ventajas:

- evitas repetir configuración
- mantienes consistencia entre operaciones similares
- puedes cambiar la estrategia en un solo punto
- es más simple testear y evolucionar la política con el tiempo

## También existe el pipeline sin tipo genérico

Si tu operación no devuelve valor, puedes usar `ResiliencePipelineBuilder` sin `T`:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(1)
    })
    .Build();

await pipeline.ExecuteAsync(async ct =>
{
    await SubirArchivoAsync(ct);
}, cancellationToken);
```

## Retry no vive solo

Aunque el foco de este artículo sea retry, la resiliencia real rara vez se resuelve con una sola estrategia.

Por ejemplo, este tipo de composición suele tener bastante sentido:

```csharp
var advancedPipeline = new ResiliencePipelineBuilder<string>()
    .AddTimeout(TimeSpan.FromSeconds(10))
    .AddRetry(new RetryStrategyOptions<string>
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(1)
    })
    .Build();
```

Aquí hay una idea importante: el retry te protege de fallos transitorios, pero el timeout te protege de operaciones que quedan colgadas demasiado tiempo.

Con el tiempo, es común sumar otras estrategias según el caso:

- `Circuit Breaker` para dejar de insistir cuando un servicio claramente está caído
- `Timeout` para no esperar indefinidamente
- `Rate Limiter` para proteger recursos internos o externos
- `Fallback` para responder de forma degradada cuando no puedes completar la operación principal

## Cuándo NO usar retry

Este punto muchas veces queda corto, pero es de los más importantes.

No deberías aplicar retry de forma automática cuando:

- el error es permanente
- la operación no es segura para repetir (no es idempotente)
- cada intento adicional empeora la congestión
- el costo de esperar más supera el beneficio
- el usuario necesita una respuesta rápida y explícita, no varios segundos de insistencia

En HTTP, eso también significa que no deberías decir "voy a reintentar cualquier `HttpRequestException` o cualquier respuesta no exitosa". Hay que distinguir entre errores de transporte y respuestas con significado funcional.

En otras palabras: resiliencia no significa esconder todos los errores. Significa tratar mejor los errores que sí vale la pena absorber.

## Qué conviene observar en producción

Si agregas retry pero no mides nada, te pierdes la mitad del valor.

Como mínimo, conviene tener visibilidad sobre esto:

- cuántos reintentos se están ejecutando
- qué excepciones están disparando esos retries
- cuánto aumenta la latencia total por los reintentos
- qué operaciones terminan fallando incluso después de agotar el pipeline

Eso te ayuda a responder preguntas concretas:

- ¿estoy absorbiendo fallos ocasionales o tapando un problema más serio?
- ¿mi política de retry está bien definida o está agregando demasiada espera?
- ¿hay un servicio externo que se está degradando más de lo que pensábamos?

Polly te deja enganchar callbacks como `OnRetry`, y desde ahí puedes registrar eventos, emitir métricas o enriquecer trazas distribuidas. En local alcanza con un `Console.WriteLine`, pero en producción lo ideal es que esos eventos terminen en tu stack de observabilidad.

## Ejemplo más cercano a un caso real

Si en vez de un servicio simulado estuvieras subiendo un archivo a Azure Blob Storage, la idea sería muy parecida:

```csharp
public class ResilientBlobUploader
{
    private readonly BlobContainerClient _container;
    private readonly ResiliencePipeline _uploadPipeline;

    public ResilientBlobUploader(BlobContainerClient container)
    {
        _container = container;
        _uploadPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder()
                    .Handle<RequestFailedException>(ex =>
                        ex.Status is 408 or 429 or 500 or 502 or 503 or 504)
                    .Handle<IOException>()
            })
            .Build();
    }

    public async Task UploadAsync(string blobName, Stream content, CancellationToken ct)
    {
        await _uploadPipeline.ExecuteAsync(async token =>
        {
            var blob = _container.GetBlobClient(blobName);
            await blob.UploadAsync(content, overwrite: true, token);
        }, ct);
    }
}
```

Observa algo importante: no se manejan todos los códigos de error, solo los que realmente sugieren una condición temporal o recuperable.

## Ejecutar el ejemplo

```bash
dotnet run
```

El programa compara operaciones con y sin resiliencia, y además muestra un pipeline inline para que se vea la diferencia entre centralizar la configuración o definirla localmente.

Conviene ejecutarlo varias veces, porque al haber fallos aleatorios vas a ver escenarios distintos en cada corrida.

## Conclusión

Lo más valioso de Polly no es que "agrega retries". Lo valioso es que te obliga a pensar mejor cómo responde tu sistema frente a fallos normales del entorno.

Si tuviera que resumir el mensaje del artículo, sería este:

1. No todos los errores son iguales.
2. Algunos errores son transitorios y merecen un retry.
3. Un retry bien configurado necesita criterio, límites, backoff y observabilidad.
4. Si la operación no es idempotente, reintentar puede ser peligroso.
5. Retry es solo una pieza dentro de una estrategia de resiliencia más amplia.

Polly v8 hace que modelar todo esto sea bastante más cómodo que antes. Y eso está bueno, porque en aplicaciones reales la resiliencia no suele ser un lujo: suele ser parte de hacer software que se comporte bien fuera de tu máquina.

## Referencias

- [Polly - repositorio oficial](https://github.com/App-vNext/Polly)
- [Polly v8 - documentación](https://www.pollydocs.org/)
- [Retry strategy - Polly docs](https://www.pollydocs.org/strategies/retry)
- [HTTP resilience en .NET](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Retry pattern - Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Resilient applications y transient faults en .NET](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/)
