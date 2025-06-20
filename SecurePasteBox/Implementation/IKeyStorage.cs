namespace SecurePasteBox.Implementation
{
    public interface IKeyStorage
    {
        Task SaveKey(string id, string key, TimeSpan expiration);
        Task<string> GetKey(string keyId);
        Task DeleteKey(string keyId);
    }
}