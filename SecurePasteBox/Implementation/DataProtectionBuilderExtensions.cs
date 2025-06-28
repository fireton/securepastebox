using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace SecurePasteBox.Implementation;

public static class DataProtectionBuilderExtensions
{
    public static IDataProtectionBuilder PersistKeysToDistributedCache(
        this IDataProtectionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
        {
            var repository = services.GetRequiredService<IXmlRepository>();

            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = repository;
            });
        });

        return builder;
    }
}
