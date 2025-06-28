using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using NanoidDotNet;

namespace SecurePasteBox.Implementation;

public class KeysManager(IDistributedCache keyStorage, IDataProtectionProvider dataProtection) : IKeysManager
{
    private const string IdAlphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int IdSize = 8;

    public async Task<string> GetAndDeleteKey(string keyId)
    {
        var key = await keyStorage.GetStringAsync(keyId);
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }
        var protector = dataProtection.CreateProtector(keyId);
        key = protector.Unprotect(key);
        await keyStorage.RemoveAsync(keyId);
        return key;
    }

    public async Task<string> SaveKey(string key, TimeSpan? expiration)
    {
        var keyId = await Nanoid.GenerateAsync(IdAlphabet, IdSize);

        var protector = dataProtection.CreateProtector(keyId);
        key = protector.Protect(key);

        await keyStorage.SetStringAsync(
            keyId, 
            key, 
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        return keyId;
    }
}
