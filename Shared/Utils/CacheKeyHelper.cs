using System.Text.Json;

namespace Shared.Utils
{
    public static class CacheKeyHelper
    {
        public static string GenerateCacheKey(string prefix, Guid id, string? fields = null)
        {
            var normalizedFields = string.IsNullOrEmpty(fields)
                ? "none" : fields;
            return $"{prefix}_{id}_{normalizedFields}";
        }

        public static string GenerateCacheKey<T>(string prefix, T parameters)
        {
            return $"{prefix}_{JsonSerializer.Serialize(parameters)}";
        }
    }
}
