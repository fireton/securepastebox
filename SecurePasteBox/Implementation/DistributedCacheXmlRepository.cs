using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace SecurePasteBox.Implementation;

#nullable enable
public class DistributedCacheXmlRepository(
    IDistributedCache cache,
    DistributedCacheXmlRepositoryOptions? options = null,
    ILoggerFactory? loggerFactory = null) : IXmlRepository
{
    private readonly IDistributedCache cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger logger = (loggerFactory ?? LoggerFactory.Create(_ => { }))
            .CreateLogger<DistributedCacheXmlRepository>();
    private readonly DistributedCacheXmlRepositoryOptions options = options ?? new DistributedCacheXmlRepositoryOptions();

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        try
        {
            var bytes = cache.Get(options.CacheKey);
            if (bytes == null)
                return [];

            var xml = Encoding.UTF8.GetString(bytes);
            var doc = XDocument.Parse(xml);
            return doc.Root is not null ? doc.Root.Elements().ToList() : [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load Data Protection keys from distributed cache.");
            return [];
        }
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        try
        {
            var elements = GetAllElements().ToList();
            elements.Add(element);

            var doc = new XDocument(new XElement("root", elements));
            var xml = doc.ToString(SaveOptions.DisableFormatting);
            var bytes = Encoding.UTF8.GetBytes(xml);

            cache.Set(options.CacheKey, bytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save Data Protection key to distributed cache.");
        }
    }
}

public class DistributedCacheXmlRepositoryOptions
{
    private const string DefaultCacheKey = "DataProtection-Keys";
    public string CacheKey { get; set; } = DefaultCacheKey;
}

