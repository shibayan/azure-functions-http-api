using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class RegexCache
{
    private static readonly ConcurrentDictionary<string, Regex> s_cache = new(StringComparer.Ordinal);

    public static bool IsMatch(string input, string pattern)
        => s_cache.GetOrAdd(pattern, static value => new Regex(value, RegexOptions.Compiled | RegexOptions.CultureInvariant)).IsMatch(input);
}
