namespace SecurePasteBox.Implementation.FilesCache;

internal sealed class FileCacheEntry
{
    public string Value { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
