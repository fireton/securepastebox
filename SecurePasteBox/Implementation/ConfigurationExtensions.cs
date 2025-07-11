﻿using System.ComponentModel;

namespace SecurePasteBox.Implementation;

public static class ConfigurationExtensions
{
    public static T GetSetting<T>(this IConfiguration configuration, string key, T defaultValue = default)
    {
        var envVarName = configuration[$"{key}:EnvVarName"];
        if (!string.IsNullOrWhiteSpace(envVarName))
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (envValue is not null)
            {
                return ConvertTo<T>(envValue, $"Environment variable '{envVarName}'");
            }
        }

        var value = configuration[key];
        if (value is not null)
        {
            return ConvertTo<T>(value, $"Configuration value at '{key}'");
        }

        var defaultFromConfig = configuration[$"{key}:DefaultValue"];
        if (defaultFromConfig is not null)
        {
            return ConvertTo<T>(defaultFromConfig, $"Default value at '{key}:DefaultValue'");
        }

        return defaultValue;
    }

    public static string GetSetting(this IConfiguration configuration, string key, string defaultValue = "")
    {
        return GetSetting<string>(configuration, key, defaultValue);
    }

    private static T ConvertTo<T>(string value, string sourceDescription)
    {
        try
        {
            var targetType = typeof(T);

            if (targetType.IsEnum)
            {
                return (T)Enum.Parse(targetType, value, ignoreCase: true);
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFromInvariantString(value);
            }

            throw new InvalidOperationException($"No suitable converter found for type {typeof(T).Name}.");
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is NotSupportedException)
        {
            throw new InvalidOperationException($"{sourceDescription} cannot be converted to type {typeof(T).Name}. Value: '{value}'", ex);
        }
    }

}
