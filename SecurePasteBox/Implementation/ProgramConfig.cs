namespace SecurePasteBox.Implementation;

public class ProgramConfig(IConfiguration configuration)
{
    public KeyStorageType KeyStorage { get; } = configuration.GetSetting("KeyStorage:Type", KeyStorageType.Memory);
    public TimeSpan DefaultExpiration { get; } = configuration.GetSetting("DefaultExpiration", TimeSpan.FromDays(7));
    public int MinIntervalSeconds { get; } = configuration.GetSetting("RateLimit:MinIntervalSeconds", 5);
    public RedisSettings Redis { get; } = new RedisSettings(configuration);
    public DataProtectionSettings DataProtection { get; } = new DataProtectionSettings(configuration);
}

public enum KeyStorageType
{
    Memory,
    Redis,
}

public class RedisSettings(IConfiguration configuration)
{
    public string Configuration { get; set; } = configuration.GetSetting("Redis:Configuration");
    public string InstanceName { get; set; } = configuration.GetSetting("Redis:InstanceName", "SecurePasteBox:");
}

public class DataProtectionSettings(IConfiguration configuration)
{
    public string ApplicationName { get; set; } = configuration.GetSetting("DataProtection:ApplicationName", "SecurePasteBox");
    public string CacheKey { get; set; } = configuration.GetSetting("DataProtection:CacheKey", "DataProtection-Keys");
}
