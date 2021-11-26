using System.Diagnostics;
using Sejil.Configuration;

namespace Sejil.Background;

internal class SchedulingHelper
{
    public static int GetTimerDuration(IReadOnlyList<RetentionPolicy> retentionPolicies)
    {
        if (!retentionPolicies.Any())
        {
            return -1;
        }

        var pfs = new Dictionary<int, List<int>>();
        foreach (var m in retentionPolicies.Select(p => (int)p.Age.TotalMinutes))
        {
            foreach (var kvp in GetPrimeFactors(m))
            {
                if (!pfs.ContainsKey(kvp.Key))
                {
                    pfs[kvp.Key] = new List<int>();
                }
                pfs[kvp.Key].Add(kvp.Value);
            }
        }

        return pfs
            .Where(p => p.Value.Count == retentionPolicies.Count)
            .ToDictionary(p => p.Key, p => p.Value.Min())
            .Aggregate(1, (result, kvp) => result * (int)Math.Pow(kvp.Key, kvp.Value));
    }

    private static Dictionary<int, int> GetPrimeFactors(int num)
    {
        Debug.Assert(num >= 2);

        var result = new List<int>();

        while (num % 2 == 0)
        {
            result.Add(2);
            num /= 2;
        }

        var factor = 3;
        while (factor * factor <= num)
        {
            if (num % factor == 0)
            {
                result.Add(factor);
                num /= factor;
            }
            else
            {
                factor += 2;
            }
        }

        if (num > 1)
        {
            result.Add(num);
        }

        return result
            .GroupBy(p => p)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
