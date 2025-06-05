namespace SecurePasteBox.Implementation;

public interface IKeysManager
{
    Task<string> SaveKey(string key);
    Task<string> GetAndDeleteKey(string keyId);
}