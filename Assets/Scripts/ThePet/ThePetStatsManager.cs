using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePetStatsManager : EntityStatsManager<ThePetStats>
{
    private const string DefaultStatsAssetName = "DefaultThePetStats";

    private ThePetStats runtimeDefaultStats;

    protected override void Start()
    {
        current_stats = ResolveStats(0);
    }

    public override void Change(int to)
    {
        if (to < 0 || stats == null || to >= stats.Length)
        {
            return;
        }

        ThePetStats resolvedStats = ResolveStats(to);
        if (resolvedStats != null && current_stats != resolvedStats)
        {
            current_stats = resolvedStats;
        }
    }

    private ThePetStats ResolveStats(int index)
    {
        if (stats == null || index < 0 || index >= stats.Length)
        {
            return null;
        }

        ThePetStats configuredStats = stats[index];
        if (configuredStats == null)
        {
            return index == 0 ? GetOrCreateRuntimeDefaultStats() : null;
        }

        if (IsDefaultStatsAsset(configuredStats))
        {
            return GetOrCreateRuntimeDefaultStats();
        }

        return configuredStats;
    }

    private ThePetStats GetOrCreateRuntimeDefaultStats()
    {
        if (runtimeDefaultStats != null)
        {
            return runtimeDefaultStats;
        }

        runtimeDefaultStats = ScriptableObject.CreateInstance<ThePetStats>();
        runtimeDefaultStats.name = $"{DefaultStatsAssetName}_Runtime";
        runtimeDefaultStats.hideFlags = HideFlags.DontSave;
        return runtimeDefaultStats;
    }

    private static bool IsDefaultStatsAsset(ThePetStats statsAsset)
    {
        return statsAsset != null && statsAsset.name == DefaultStatsAssetName;
    }
}
