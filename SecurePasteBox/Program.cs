using System.Net;
using Microsoft.Extensions.FileProviders;
using SecurePasteBox.Implementation;

namespace SecurePasteBox;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var env = builder.Environment;
        var programConfig = new ProgramConfig(builder.Configuration);

        builder.Services.AddSingleton(programConfig);
        builder.Services.AddMemoryCache();
        builder.Services.AddUniversalRateLimiter(TimeSpan.FromSeconds(programConfig.MinIntervalSeconds));

        builder.Services.AddSingleton<IKeysManager, KeysManager>();

        switch (programConfig.KeyStorage)
        {
            case KeyStorageType.MemoryCache:
                builder.Services.AddSingleton<IKeyStorage, MemoryCacheKeyStorage>();
                break;
            case KeyStorageType.Redis:
                throw new NotImplementedException("Redis key storage is not implemented yet.");
                // builder.Services.AddSingleton<IKeyStorage, RedisKeyStorage>();
                break;
            case KeyStorageType.SQLite:
                throw new NotImplementedException("SQLite key storage is not implemented yet.");
                // builder.Services.AddSingleton<IKeyStorage, SQLiteKeyStorage>();
                break;
            default:
                throw new NotSupportedException($"Unsupported key storage type: {programConfig.KeyStorage}");
        }

        builder.Services.AddSingleton<IKeyStorage, MemoryCacheKeyStorage>();

        var app = builder.Build();
        app.UseRateLimiter();

        app.MapGet("/api/health", () => Results.Ok("Healthy"));

        app.MapPost("/api/keys", async (HttpContext context) =>
        {
            var keysManager = context.RequestServices.GetRequiredService<IKeysManager>();

            var body = await context.Request.ReadFromJsonAsync<KeyRequest>();
            if (body is null || string.IsNullOrWhiteSpace(body.Key))
            {
                return Results.BadRequest("Key cannot be empty.");
            }

            var keyId = await keysManager.SaveKey(
                body.Key,
                body.Expiration);

            return Results.Ok(new { KeyId = keyId });
        });

        app.MapDelete("/api/keys/{keyId}", async (string keyId, HttpContext context) =>
        {
            var keysManager = context.RequestServices.GetRequiredService<IKeysManager>();
            var key = await keysManager.GetAndDeleteKey(keyId);
            if (string.IsNullOrEmpty(key))
            {
                return Results.NotFound("Key not found, expired or already deleted.");
            }
            return Results.Ok(new { Key = key });
        }).WithKeyRetrievalRateLimit();

        var pagesPath = env.IsDevelopment() // for easier development
            ? Path.Combine(env.ContentRootPath, "Pages") 
            : Path.Combine(AppContext.BaseDirectory, "Pages");

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(pagesPath),
            RequestPath = ""
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(pagesPath),
            RequestPath = ""
        });

        app.MapFallback(() =>
        {
            var indexPath = Path.Combine(pagesPath, "index.html");
            return Results.File(indexPath, "text/html");
        });

        app.Run();
    }

    private sealed record KeyRequest(string Key, TimeSpan? Expiration);
}
