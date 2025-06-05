using NanoidDotNet;

namespace SecurePasteBox.Implementation;

public class KeysManager(IKeyStorage keyStorage) : IKeysManager
{
    private const string IdAlphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int IdSize = 8;

    public async Task<string> GetAndDeleteKey(string keyId)
    {
        var key = await keyStorage.GetKey(keyId);
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }
        await keyStorage.DeleteKey(keyId);
        return key;
    }

    public async Task<string> SaveKey(string key)
    {
        var keyId = await Nanoid.GenerateAsync(IdAlphabet, IdSize);
        await keyStorage.SaveKey(keyId, key);
        return keyId;
    }
}
