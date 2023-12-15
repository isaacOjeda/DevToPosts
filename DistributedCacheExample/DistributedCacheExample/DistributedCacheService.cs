using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DistributedCacheExample;

public class DistributedCacheService(IDistributedCache distributedCache)
{
    public async Task<T?> GetCachedItem<T>(string key)
        where T : class
    {
        var dataInBytes = await distributedCache.GetAsync(key);

        if (dataInBytes is null)
        {
            return null;
        }

        var rawJson = System.Text.Encoding.UTF8.GetString(dataInBytes);

        return JsonSerializer.Deserialize<T>(rawJson);
    }

    public async Task SaveItem<T>(T item, string key, int expirationInMinutes)
    {
        var dataJson = JsonSerializer.Serialize(item);
        var dataInBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);

        await distributedCache.SetAsync(key, dataInBytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(expirationInMinutes)
        });
    }
}

