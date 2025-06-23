namespace SecurePasteBox.Implementation;

public class ProgramConfig(IConfiguration configuration)
{
    public KeyStorageType KeyStorage { get; } = configuration.GetSetting("KeyStorage:Type", KeyStorageType.MemoryCache);
    public TimeSpan DefaultExpiration { get; } = configuration.GetSetting("DefaultExpiration", TimeSpan.FromDays(7));
    public int MinIntervalSeconds { get; } = configuration.GetSetting("RateLimit:MinIntervalSeconds", 5);
}

public enum KeyStorageType
{
    MemoryCache,
    Redis,
    SQLite
}
