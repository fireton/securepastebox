namespace SecurePasteBox.Implementation;

public class ProgramConfig(IConfiguration configuration)
{
    public KeyStorageType KeyStorage { get; } = configuration.GetSetting("KeyStorage:Type", KeyStorageType.Memory);
    public TimeSpan DefaultExpiration { get; } = configuration.GetSetting("DefaultExpiration", TimeSpan.FromDays(7));
    public int MinIntervalSeconds { get; } = configuration.GetSetting("RateLimit:MinIntervalSeconds", 5);
    public FilesSettings Files { get; } = new FilesSettings(configuration);
    public DataProtectionSettings DataProtection { get; } = new DataProtectionSettings(configuration);
}

public enum KeyStorageType
{
    Memory,
    Files,
}

public class FilesSettings(IConfiguration configuration)
{
    public string DataDirectory { get; set; } = configuration.GetSetting("Files:DataDirectory", "/data");
    public TimeSpan CleanupInterval { get; set; } = configuration.GetSetting("Files:CleanupInterval", TimeSpan.FromMinutes(5));
}

public class DataProtectionSettings(IConfiguration configuration)
{
    public string ApplicationName { get; set; } = configuration.GetSetting("DataProtection:ApplicationName", "SecurePasteBox");
    public string CacheKey { get; set; } = configuration.GetSetting("DataProtection:CacheKey", "DataProtection-Keys");
}
