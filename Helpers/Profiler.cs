using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.Helpers;

public static class Profiler
{
    public const bool ENABLED = true;
    private const int INTERVAL = 20;

    private static readonly Dictionary<string, float> startTimes = [];
    private static readonly Dictionary<string, float> elapsedTimes = [];
    private static readonly List<Dictionary<string, float>> lastTimes = [];
    private static readonly Dictionary<string, float> averageTimes = [];

    public static void NewFrame()
    {
        if (!ENABLED) return;

        lastTimes.Add(new(elapsedTimes));
        startTimes.Clear();
        elapsedTimes.Clear();

        if (lastTimes.Count >= INTERVAL)
        {
            averageTimes.Clear();
            Dictionary<string, int> counts = [];

            foreach (Dictionary<string, float> times in lastTimes)
            {
                foreach (KeyValuePair<string, float> pair in times)
                {
                    averageTimes.TryAdd(pair.Key, 0);
                    averageTimes[pair.Key] += pair.Value;
                    counts.TryAdd(pair.Key, 0);
                    counts[pair.Key]++;
                }
            }

            foreach (string name in counts.Keys)
            {
                averageTimes[name] /= counts[name];
            }

            lastTimes.Clear();
        }
    }

    public static void Begin(string name)
    {
        if (!ENABLED) return;
        startTimes.Add(name, Time.realtimeSinceStartup);
    }

    public static void End(string name)
    {
        if (!ENABLED) return;
        float elapsed = Time.realtimeSinceStartup - startTimes[name];
        elapsedTimes.TryAdd(name, 0);
        elapsedTimes[name] += elapsed;
    }

    public static Dictionary<string, float> GetTimes()
    {
        return averageTimes;
    }
}