using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;

namespace SecurePasteBox.Implementation.FilesCache;

public class FileExpiredCacheCleaner(string basePath, TimeSpan? interval = null, ILogger<FileExpiredCacheCleaner> logger = null!) : BackgroundService
{
    private readonly ILogger<FileExpiredCacheCleaner> logger = logger ?? NullLogger<FileExpiredCacheCleaner>.Instance;
    private readonly TimeSpan cleanupInterval = interval ?? TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Expired cache cleaner started for: {Path}", basePath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                foreach (var file in Directory.EnumerateFiles(basePath))
                {
                    try
                    {
                        var text = await File.ReadAllTextAsync(file, stoppingToken);
                        var entry = JsonSerializer.Deserialize<FileCacheEntry>(text);

                        if (entry?.ExpiresAt is DateTime expiresAt && expiresAt < now)
                        {
                            File.Delete(file);
                            logger.LogInformation("Deleted expired file: {File}", file);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to process cache file: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during cache cleanup loop");
            }

            await Task.Delay(cleanupInterval, stoppingToken);
        }

        logger.LogInformation("Expired cache cleaner stopped.");
    }
}


