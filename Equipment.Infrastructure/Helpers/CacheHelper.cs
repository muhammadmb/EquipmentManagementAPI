using System.Text.Json;

namespace Infrastructure.Helpers
{
    public static class CacheHelper
    {
        public static string DateKey(DateTime? date)
        {
            if (!date.HasValue)
                return "null";

            return date.Value
                .ToUniversalTime()
                .ToString("yyyyMMddHHmmss");
        }

        public static string AnalyticsKey(
            string key,
            string? version = null,
            params (string Key, object? Value)[] parameters)
        {
            var suffix = string.Join(
                    ":",
                    parameters.Select(p => $"{p.Key}={p.Value ?? "null"}"));

            return $"analytics:v{version}:{key}:{suffix}";
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value);
        }

        public static T? Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}
