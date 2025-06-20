
using Microsoft.Extensions.Caching.Memory;

namespace SecurePasteBox.Implementation;

public class MemoryCacheKeyStorage(IMemoryCache memoryCache) : IKeyStorage
{
    public Task DeleteKey(string keyId)
    {
        memoryCache.Remove(keyId);
        return Task.CompletedTask;
    }

    public Task<string> GetKey(string keyId)
    {
        return Task.FromResult(memoryCache.TryGetValue(keyId, out string value) ? value : null);
    }

    public Task SaveKey(string id, string key, TimeSpan expiration)
    {
        memoryCache.Set(id, key, expiration);
        return Task.CompletedTask;
    }
}
