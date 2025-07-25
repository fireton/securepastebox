﻿namespace SecurePasteBox.Implementation;

public class ProgramConfig(IConfiguration configuration)
{
    public KeyStorageType KeyStorage { get; } = configuration.GetSetting("KeyStorage:Type", KeyStorageType.Memory);
    public TimeSpan DefaultExpiration { get; } = configuration.GetSetting("DefaultExpiration", TimeSpan.FromDays(7));
    public int MinIntervalSeconds { get; } = configuration.GetSetting("RateLimit:MinIntervalSeconds", 5);
    public FilesSettings Files { get; } = new FilesSettings(configuration);
    public DataProtectionSettings DataProtection { get; } = new DataProtectionSettings(configuration);
    public string Theme { get; } = configuration.GetSetting("FrontEnd:Theme", "Basic");
    public Dictionary<string, object> FrontEndSettings { get; } = ReadFrontendSettings(configuration);

    private static Dictionary<string, object> ReadFrontendSettings(IConfiguration configuration)
    {
        var theme = configuration.GetSetting("FrontEnd:Theme", "Basic");
        var section = configuration.GetSection($"FrontEnd:{theme}");

        if (!section.Exists())
        {
            throw new InvalidOperationException($"Theme '{theme}' not found in configuration.");
        }

        return section.GetChildren()
            .ToDictionary(
                child => child.Key,
                child => configuration.GetSetting(child.Path) ?? string.Empty as object,
                StringComparer.OrdinalIgnoreCase);

    }
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
