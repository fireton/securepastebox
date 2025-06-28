using Microsoft.Extensions.Caching.Distributed;

namespace SecurePasteBox.Implementation.FilesCache;

public static class ServiceExtensions
{
    public static IServiceCollection AddFileDistributedCache(this IServiceCollection services, string basePath, TimeSpan cleaningInterval)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException("Base path cannot be null or empty.", nameof(basePath));
        }
        services.AddSingleton<IDistributedCache>(new FileDistributedCache(basePath));
        services.AddSingleton<IHostedService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<FileExpiredCacheCleaner>>();
            return new FileExpiredCacheCleaner(basePath, cleaningInterval, logger);
        });

        return services;
    }
}
