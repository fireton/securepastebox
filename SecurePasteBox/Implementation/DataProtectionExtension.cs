using Microsoft.AspNetCore.DataProtection;

namespace SecurePasteBox.Implementation;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddConfiguredDataProtection(
        this IServiceCollection services,
        IConfiguration config)
    {
        var options = new 
        {
            ApplicationName = config.GetSetting("DataProtection:ApplicationName"),
            KeyLifetimeDays = config.GetSetting<TimeSpan>("DataProtection:KeyLifetimeDays"),
            KeyStorage = new 
            {
                Type = config.GetSetting<KeyStorageType>("DataProtection:KeyStorage:Type"),
                Path = config.GetSetting("DataProtection:KeyStorage:FilePath", "./keys"),
                RedisConnectionString = config.GetSetting("DataProtection:KeyStorage:RedisConnectionString")
            }
        };
        var builder = services.AddDataProtection()
            .SetApplicationName(options.ApplicationName)
            .SetDefaultKeyLifetime(options.KeyLifetimeDays);

        switch (options.KeyStorage.Type)
        {
            case KeyStorageType.FileSystem:
                builder.PersistKeysToFileSystem(new DirectoryInfo(options.KeyStorage.Path));
                break;
            
            case KeyStorageType.Redis:
                var redis = ConnectionMultiplexer.Connect(options.KeyStorage.RedisConnectionString);
                builder.PersistKeysToStackExchangeRedis(redis, "SecurePasteBox-Keys");
                break;

            case KeyStorageType.Memory:
                // Keys lost on restart
                break;

            default:
                throw new InvalidOperationException($"Unknown key storage: {options.KeyStorage.Type}");
        }

        return services;
    }
}

public enum KeyStorageType
{
    FileSystem,
    Redis,
    Memory
}

