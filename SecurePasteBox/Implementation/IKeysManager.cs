namespace SecurePasteBox.Implementation;

public interface IKeysManager
{
    Task<string> SaveKey(string key, TimeSpan? expiration);
    Task<string> GetAndDeleteKey(string keyId);
}