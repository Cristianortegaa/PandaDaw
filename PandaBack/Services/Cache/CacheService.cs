using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace PandaBack.Services.Cache;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _redisCache; // Para Producción
    private readonly IMemoryCache _memoryCache;     // Para Desarrollo
    private readonly bool _useRedis;

    public CacheService(IDistributedCache redisCache, IMemoryCache memoryCache, IConfiguration config)
    {
        _redisCache = redisCache;
        _memoryCache = memoryCache;
        // Leemos del appsettings si debemos usar Redis
        _useRedis = config.GetValue<bool>("UseRedisCache"); 
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (_useRedis)
        {
            var data = await _redisCache.GetStringAsync(key);
            return data is null ? default : JsonSerializer.Deserialize<T>(data);
        }
        
        return _memoryCache.TryGetValue(key, out T? value) ? value : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        if (_useRedis)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
            var json = JsonSerializer.Serialize(value);
            await _redisCache.SetStringAsync(key, json, options);
        }
        else
        {
            _memoryCache.Set(key, value, expiration);
        }
    }

    public Task RemoveAsync(string key)
    {
        if (_useRedis) return _redisCache.RemoveAsync(key);
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}