using Azure.ResourceManager;
using Microsoft.Extensions.Caching.Memory;

namespace Bancey.Bot.WorkerService.Azure;

public class ResourceCacheManager<T>(ILogger<ResourceCacheManager<T>> logger, IMemoryCache cache) where T : ArmResource
{
    private ILogger<ResourceCacheManager<T>> _logger = logger;
    private IMemoryCache _cache = cache;
    private static readonly IList<string> _cachedKeys = new List<string>();

    public IList<string> CachedKeys()
    {
        return _cachedKeys;
    }

    public void CacheResource(string key, T item, MemoryCacheEntryOptions options)
    {
        options.RegisterPostEvictionCallback(OnPostEviction, _logger);
        _cache.Set(key, item, options);
        _cachedKeys.Add(key);
    }

    public bool TryGetValue(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public static void OnPostEviction(object key, object? value, EvictionReason reason, object? state)
    {
        ILogger? logger = (ILogger?)state;
        if (value is T item && key is string keyStr)
        {
            logger?.LogInformation("{type} {id} evicted from cache. Reason: {reason}", typeof(T).Name, item.Id, reason);
            _cachedKeys.Remove(keyStr);
        }
    }
}