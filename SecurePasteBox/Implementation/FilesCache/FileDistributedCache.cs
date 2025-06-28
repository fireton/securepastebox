using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SecurePasteBox.Implementation.FilesCache;

#nullable enable
public class FileDistributedCache : IDistributedCache
{
    private readonly string basePath;

    public FileDistributedCache(string basePath)
    {
        this.basePath = basePath;
        Directory.CreateDirectory(this.basePath);
    }

    public byte[]? Get(string key) =>
        GetAsync(key).GetAwaiter().GetResult();

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        var path = GetPath(key);
        if (!File.Exists(path)) return null;

        var entry = await File.ReadAllTextAsync(path, token);
        var cached = JsonSerializer.Deserialize<FileCacheEntry>(entry);

        if (cached?.ExpiresAt < DateTime.UtcNow)
        {
            File.Delete(path);
            return null;
        }

        return Convert.FromBase64String(cached!.Value);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        SetAsync(key, value, options).GetAwaiter().GetResult();

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        var path = GetPath(key);
        DateTime? expiresAt = null;
        
        if (options.AbsoluteExpiration.HasValue)
        {
            expiresAt = options.AbsoluteExpiration.Value.UtcDateTime;
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            expiresAt = DateTime.UtcNow + options.AbsoluteExpirationRelativeToNow.Value;
        }

        var entry = new FileCacheEntry
        {
            Value = Convert.ToBase64String(value),
            ExpiresAt = expiresAt,
        };

        var json = JsonSerializer.Serialize(entry);
        await File.WriteAllTextAsync(path, json, token);
    }

    public void Remove(string key) => File.Delete(GetPath(key));
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        File.Delete(GetPath(key));
        return Task.CompletedTask;
    }

    public void Refresh(string key) { } // not implemented
    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    private string GetPath(string key) =>
        Path.Combine(basePath, WebUtility.UrlEncode(key));
}

