namespace SecurePasteBox.Implementation
{
    public interface IKeyStorage
    {
        Task SaveKey(string id, string key);
        Task<string> GetKey(string keyId);
        Task DeleteKey(string keyId);
    }
}