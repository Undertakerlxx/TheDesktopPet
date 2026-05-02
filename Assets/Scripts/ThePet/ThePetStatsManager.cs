using System.Collections;
using System.Collections.Generic;
using DesktopPet.Save;
using UnityEngine;

public class ThePetStatsManager : EntityStatsManager<ThePetStats>
{
    private const string DefaultStatsAssetName = "DefaultThePetStats";

    [System.Serializable]
    private class InspectorDebugStats
    {
        public float intimacy = 30f;
        public float happiness = 66f;
        public float energy = 65f;
        public float energy_max = 100f;
        public float focus = 100f;
        public float satiety = 60f;

        public void ApplyTo(ThePetStats stats)
        {
            if (stats == null)
            {
                return;
            }

            stats.intimacy = intimacy;
            stats.happiness = happiness;
            stats.energy = energy;
            stats.energy_max = energy_max;
            stats.focus = focus;
            stats.satiety = satiety;
        }
    }

    [Header("Debug")]
    [SerializeField] private bool ignoreSavedStatsInEditor = true;
    [SerializeField] private bool useInspectorDebugStatsInEditor = true;
    [SerializeField] private InspectorDebugStats inspectorDebugStats = new();

    private ThePetStats runtimeStats;

    protected override void Start()
    {
        current_stats = CreateRuntimeStats(ResolveStatsAsset(0));
        LoadCurrentStats();
    }

    public override void Change(int to)
    {
        if (to < 0 || stats == null || to >= stats.Length)
        {
            return;
        }

        ThePetStats nextStats = CreateRuntimeStats(ResolveStatsAsset(to));
        if (nextStats != null)
        {
            current_stats = nextStats;
            LoadCurrentStats();
        }
    }

    public void SaveCurrentStats()
    {
        if (current_stats == null)
        {
            return;
        }

        if (ShouldIgnoreSavedStatsInCurrentEnvironment())
        {
            return;
        }

        GameSaveManager.Instance.SaveStats(current_stats);
    }

    protected virtual void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentStats();
        }
    }

    protected virtual void OnApplicationQuit()
    {
        SaveCurrentStats();
    }

    private void LoadCurrentStats()
    {
        if (current_stats == null)
        {
            return;
        }

        if (ShouldIgnoreSavedStatsInCurrentEnvironment())
        {
            return;
        }

        ThePetStatsSaveData saveData = GameSaveManager.Instance.LoadStats();
        saveData?.ApplyTo(current_stats);
    }

    private ThePetStats ResolveStatsAsset(int index)
    {
        if (stats == null || index < 0 || index >= stats.Length)
        {
            return null;
        }

        return stats[index];
    }

    private ThePetStats CreateRuntimeStats(ThePetStats sourceStats)
    {
        if (runtimeStats != null)
        {
            Destroy(runtimeStats);
        }

        runtimeStats = ScriptableObject.CreateInstance<ThePetStats>();
        runtimeStats.hideFlags = HideFlags.DontSave;

        if (sourceStats != null && !IsDefaultStatsAsset(sourceStats))
        {
            runtimeStats.intimacy = sourceStats.intimacy;
            runtimeStats.happiness = sourceStats.happiness;
            runtimeStats.energy = sourceStats.energy;
            runtimeStats.energy_max = sourceStats.energy_max;
            runtimeStats.focus = sourceStats.focus;
            runtimeStats.satiety = sourceStats.satiety;
            runtimeStats.name = $"{sourceStats.name}_Runtime";
        }
        else
        {
            runtimeStats.name = $"{DefaultStatsAssetName}_Runtime";
        }

        ApplyInspectorDebugStatsIfNeeded(runtimeStats);
        return runtimeStats;
    }

    private static bool IsDefaultStatsAsset(ThePetStats statsAsset)
    {
        return statsAsset != null && statsAsset.name == DefaultStatsAssetName;
    }

    private bool ShouldIgnoreSavedStatsInCurrentEnvironment()
    {
#if UNITY_EDITOR
        return ignoreSavedStatsInEditor;
#else
        return false;
#endif
    }

    private void ApplyInspectorDebugStatsIfNeeded(ThePetStats stats)
    {
#if UNITY_EDITOR
        if (!useInspectorDebugStatsInEditor || inspectorDebugStats == null)
        {
            return;
        }

        inspectorDebugStats.ApplyTo(stats);
#endif
    }
}
