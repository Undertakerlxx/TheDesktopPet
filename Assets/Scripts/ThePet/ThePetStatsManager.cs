using System.Collections;
using System.Collections.Generic;
using DesktopPet.MiniGame;
using DesktopPet.Save;
using UnityEngine;

public class ThePetStatsManager : EntityStatsManager<ThePetStats>
{
    private const string DefaultStatsAssetName = "DefaultThePetStats";
    private const float MaxStatValue = 100f;
    private const float MaxFocusValue = 500f;
    private const int MiniGameSuccessIntimacyReward = 2;
    private const int MiniGameDailyIntimacyCap = 100;

    [System.Serializable]
    private class InspectorDebugStats
    {
        public float intimacy = 30f;
        public float happiness = 66f;
        public float energy = 200f;
        public float energy_max = 200f;
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

    [System.Serializable]
    private class AutoSaveSettings
    {
        public bool enabled = true;
        [Min(1f)] public float intervalSeconds = 15f;

        public void Sanitize()
        {
            intervalSeconds = Mathf.Max(1f, intervalSeconds);
        }
    }

    [Header("Debug")]
    [SerializeField] private bool ignoreSavedStatsInEditor = true;
    [SerializeField] private bool useInspectorDebugStatsInEditor = true;
    [SerializeField] private InspectorDebugStats inspectorDebugStats = new();

    [Header("Happiness Decay")]
    [SerializeField] private bool enableHappinessDecay = true;
    [SerializeField] private ThePetHappinessDecaySettings happinessDecaySettings = new();

    [Header("Energy Recovery")]
    [SerializeField] private bool enableEnergyRecovery = true;
    [SerializeField] private ThePetEnergyRecoverySettings energyRecoverySettings = new();

    [Header("Satiety Decay")]
    [SerializeField] private bool enableSatietyDecay = true;
    [SerializeField] private ThePetSatietyDecaySettings satietyDecaySettings = new();

    [Header("Auto Save")]
    [SerializeField] private AutoSaveSettings autoSaveSettings = new();

    private ThePet pet;
    private ThePetStats runtimeStats;
    private bool hasUnsavedChanges;
    private float autoSaveElapsedSeconds;
    private ThePetEnergyRecoveryProfile? cachedRecoveryProfile;

    private void Awake()
    {
        pet = GetComponent<ThePet>();
    }

    protected override void Start()
    {
        current_stats = CreateRuntimeStats(ResolveStatsAsset(0));
        ResetRuntimeTracking();
        LoadCurrentStats();
    }

    private void Update()
    {
        TickHappinessDecay();
        TickEnergyRecovery();
        TickSatietyDecay();
        TickAutoSave();
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
            ResetRuntimeTracking();
            LoadCurrentStats();
        }
    }

    public void NotifyStatsChanged()
    {
        hasUnsavedChanges = true;
    }

    public void ApplyMiniGameResult(MiniGameKind gameKind, bool success, bool brokeRecord, int score, float completionSeconds = -1f)
    {
        if (current_stats == null)
        {
            return;
        }

        float happinessDelta = success ? 5f : -2f;
        if (brokeRecord)
        {
            happinessDelta += 3f;
        }

        current_stats.happiness = Mathf.Clamp(current_stats.happiness + happinessDelta, 0f, MaxStatValue);
        current_stats.energy = Mathf.Clamp(current_stats.energy - GetMiniGameEnergyCost(gameKind), 0f, current_stats.energy_max);

        float focusGain = GetMiniGameFocusGain(gameKind, score, completionSeconds);
        if (focusGain > 0f)
        {
            current_stats.focus = Mathf.Clamp(current_stats.focus + focusGain, 0f, MaxFocusValue);
        }

        if (success)
        {
            ApplyMiniGameIntimacyReward();
        }

        NotifyStatsChanged();
    }

    public bool CanStartMiniGame(MiniGameKind gameKind, out string reason)
    {
        reason = string.Empty;
        if (!IsMiniGameUnlocked(gameKind, out int requiredIntimacy))
        {
            reason = $"亲密度达到 {requiredIntimacy} 后解锁该小游戏。";
            return false;
        }

        return CanStartMiniGame(out reason);
    }

    public bool CanStartMiniGame(out string reason)
    {
        reason = string.Empty;
        if (current_stats == null)
        {
            return true;
        }

        if (current_stats.satiety < 30f)
        {
            reason = "\u9965\u997f\u503c\u4f4e\u4e8e30\uff0c\u65e0\u6cd5\u8fdb\u884c\u5c0f\u6e38\u620f\u3002";
            return false;
        }

        if (current_stats.satiety >= 50f && current_stats.energy <= 50f)
        {
            reason = "\u6d3b\u529b\u503c\u8fc7\u4f4e\uff0c\u65e0\u6cd5\u8fdb\u884c\u5c0f\u6e38\u620f\u3002";
            return false;
        }

        return true;
    }

    public bool IsMiniGameUnlocked(MiniGameKind gameKind)
    {
        return IsMiniGameUnlocked(gameKind, out _);
    }

    public bool IsMiniGameUnlocked(MiniGameKind gameKind, out int requiredIntimacy)
    {
        requiredIntimacy = GetMiniGameUnlockRequirement(gameKind);
        return current_stats == null || current_stats.intimacy >= requiredIntimacy;
    }

    public static int GetMiniGameUnlockRequirement(MiniGameKind gameKind)
    {
        return gameKind switch
        {
            MiniGameKind.GeometryAtAGlance => 200,
            MiniGameKind.DinoRun => 500,
            MiniGameKind.DodgeBall => 1000,
            _ => 0
        };
    }

    public int GetMiniGameScoreModifierPercent()
    {
        if (current_stats == null)
        {
            return 0;
        }

        if (current_stats.satiety < 50f)
        {
            return current_stats.satiety >= 30f ? -10 : 0;
        }

        if (current_stats.energy >= 160f)
        {
            return 10;
        }

        if (current_stats.energy >= 100f)
        {
            return 0;
        }

        if (current_stats.energy >= 50f)
        {
            return -10;
        }

        return 0;
    }

    public string GetMiniGameScoreModifierLabel()
    {
        if (current_stats == null)
        {
            return string.Empty;
        }

        if (current_stats.satiety < 50f)
        {
            return current_stats.satiety >= 30f ? "\u9965\u997f\u72b6\u6001\u60e9\u7f5a" : string.Empty;
        }

        if (current_stats.energy >= 160f)
        {
            return "\u9ad8\u6d3b\u529b\u52a0\u6210";
        }

        if (current_stats.energy >= 100f)
        {
            return string.Empty;
        }

        if (current_stats.energy >= 50f)
        {
            return "\u4f4e\u6d3b\u529b\u60e9\u7f5a";
        }

        return string.Empty;
    }

    public bool SaveCurrentStats()
    {
        if (current_stats == null)
        {
            return false;
        }

        if (ShouldIgnoreSavedStatsInCurrentEnvironment())
        {
            return false;
        }

        bool saved = GameSaveManager.Instance.SaveStats(current_stats);
        if (saved)
        {
            ResetDirtyState();
        }

        return saved;
    }

    /// <summary>
    /// Applies the stat recovery produced by feeding the pet.
    /// </summary>
    /// <param name="satietyRestore">The amount of satiety to restore.</param>
    /// <param name="happinessRestore">The amount of happiness to restore.</param>
    /// <param name="intimacyRestore">The amount of intimacy to restore.</param>
    /// <returns><see langword="true"/> if runtime stats were available and updated; otherwise, <see langword="false"/>.</returns>
    public bool ApplyFeedingEffect(float satietyRestore, float happinessRestore, float intimacyRestore)
    {
        if (current_stats == null)
        {
            return false;
        }

        current_stats.satiety = Mathf.Clamp(current_stats.satiety + satietyRestore, 0f, MaxStatValue);
        current_stats.happiness = Mathf.Clamp(current_stats.happiness + happinessRestore, 0f, MaxStatValue);
        current_stats.intimacy = Mathf.Max(0f, current_stats.intimacy + intimacyRestore);

        NotifyStatsChanged();
        SaveCurrentStats();
        return true;
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

        if (!ShouldIgnoreSavedStatsInCurrentEnvironment())
        {
            ThePetStatsSaveData saveData = GameSaveManager.Instance.LoadStats();
            saveData?.ApplyTo(current_stats);
        }

        EnsureMiniGameIntimacyProgressCurrent();
        ClampRuntimeStats(current_stats);
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
            runtimeStats.miniGameDailyIntimacyGain = sourceStats.miniGameDailyIntimacyGain;
            runtimeStats.miniGameDailyIntimacyDate = sourceStats.miniGameDailyIntimacyDate;
            runtimeStats.name = $"{sourceStats.name}_Runtime";
        }
        else
        {
            runtimeStats.name = $"{DefaultStatsAssetName}_Runtime";
        }

        if (runtimeStats.energy_max <= 100f && runtimeStats.energy <= 100f)
        {
            runtimeStats.energy_max = 200f;
            runtimeStats.energy = 200f;
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

    private void TickHappinessDecay()
    {
        if (!enableHappinessDecay || current_stats == null)
        {
            return;
        }

        float totalDecayPerMinute = happinessDecaySettings.GetTotalDecayPerMinute(current_stats.satiety);
        if (totalDecayPerMinute <= 0f)
        {
            return;
        }

        float nextHappiness = Mathf.Clamp(
            current_stats.happiness - totalDecayPerMinute * Time.deltaTime / 60f,
            0f,
            MaxStatValue);

        if (Mathf.Approximately(nextHappiness, current_stats.happiness))
        {
            return;
        }

        current_stats.happiness = nextHappiness;
        NotifyStatsChanged();
    }

    private void TickEnergyRecovery()
    {
        if (!enableEnergyRecovery || current_stats == null)
        {
            return;
        }

        float recoveryPerMinute = energyRecoverySettings.GetRecoveryPerMinute(ResolveEnergyRecoveryProfile());
        if (recoveryPerMinute <= 0f || current_stats.energy >= current_stats.energy_max)
        {
            return;
        }

        float nextEnergy = Mathf.Clamp(
            current_stats.energy + recoveryPerMinute * Time.deltaTime / 60f,
            0f,
            current_stats.energy_max);

        if (Mathf.Approximately(nextEnergy, current_stats.energy))
        {
            return;
        }

        current_stats.energy = nextEnergy;
        NotifyStatsChanged();
    }

    private void TickSatietyDecay()
    {
        if (!enableSatietyDecay || current_stats == null)
        {
            return;
        }

        float decayPerMinute = satietyDecaySettings.decayPerMinute;
        if (decayPerMinute <= 0f || current_stats.satiety <= 0f)
        {
            return;
        }

        float nextSatiety = Mathf.Clamp(
            current_stats.satiety - decayPerMinute * Time.deltaTime / 60f,
            0f,
            MaxStatValue);

        if (Mathf.Approximately(nextSatiety, current_stats.satiety))
        {
            return;
        }

        current_stats.satiety = nextSatiety;
        NotifyStatsChanged();
    }

    private void TickAutoSave()
    {
        if (!autoSaveSettings.enabled || !hasUnsavedChanges || ShouldIgnoreSavedStatsInCurrentEnvironment())
        {
            return;
        }

        autoSaveElapsedSeconds += Time.deltaTime;
        if (autoSaveElapsedSeconds >= autoSaveSettings.intervalSeconds)
        {
            autoSaveElapsedSeconds = 0f;
            SaveCurrentStats();
        }
    }

    private void ResetDirtyState()
    {
        hasUnsavedChanges = false;
        autoSaveElapsedSeconds = 0f;
    }

    private void ApplyMiniGameIntimacyReward()
    {
        EnsureMiniGameIntimacyProgressCurrent();

        int remainingReward = MiniGameDailyIntimacyCap - current_stats.miniGameDailyIntimacyGain;
        if (remainingReward <= 0)
        {
            return;
        }

        int reward = Mathf.Min(MiniGameSuccessIntimacyReward, remainingReward);
        current_stats.intimacy += reward;
        current_stats.miniGameDailyIntimacyGain += reward;
    }

    private void EnsureMiniGameIntimacyProgressCurrent()
    {
        if (current_stats == null)
        {
            return;
        }

        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        if (current_stats.miniGameDailyIntimacyDate == today)
        {
            return;
        }

        current_stats.miniGameDailyIntimacyDate = today;
        current_stats.miniGameDailyIntimacyGain = 0;
    }

    private static float GetMiniGameEnergyCost(MiniGameKind gameKind)
    {
        return gameKind switch
        {
            MiniGameKind.SchulteGrid => 4f,
            MiniGameKind.ColorGrid => 3f,
            MiniGameKind.EyeHandSpeed => 5f,
            MiniGameKind.GeometryAtAGlance => 6f,
            MiniGameKind.DinoRun => 8f,
            MiniGameKind.DodgeBall => 10f,
            _ => 0f
        };
    }

    private static float GetMiniGameFocusGain(MiniGameKind gameKind, int score, float completionSeconds)
    {
        return gameKind switch
        {
            MiniGameKind.SchulteGrid => GetSchulteFocusGain(completionSeconds),
            MiniGameKind.ColorGrid => GetScoreTierFocusGain(score),
            MiniGameKind.EyeHandSpeed => GetScoreTierFocusGain(score),
            MiniGameKind.GeometryAtAGlance => GetScoreTierFocusGain(score),
            _ => 0f
        };
    }

    private static float GetSchulteFocusGain(float completionSeconds)
    {
        if (completionSeconds <= 0f)
        {
            return 0f;
        }

        if (completionSeconds < 45f)
        {
            return 6f;
        }

        if (completionSeconds <= 60f)
        {
            return 4f;
        }

        return 3f;
    }

    private static float GetScoreTierFocusGain(int score)
    {
        if (score >= 1000)
        {
            return 4f;
        }

        if (score >= 500)
        {
            return 3f;
        }

        if (score >= 100)
        {
            return 2f;
        }

        return 0f;
    }

    private void ResetRuntimeTracking()
    {
        ResetDirtyState();
        cachedRecoveryProfile = null;
    }

    private static void ClampRuntimeStats(ThePetStats stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.energy_max = Mathf.Max(0f, stats.energy_max);
        stats.happiness = Mathf.Clamp(stats.happiness, 0f, MaxStatValue);
        stats.energy = Mathf.Clamp(stats.energy, 0f, stats.energy_max);
        stats.focus = Mathf.Clamp(stats.focus, 0f, MaxFocusValue);
        stats.satiety = Mathf.Clamp(stats.satiety, 0f, MaxStatValue);
    }

    private ThePetEnergyRecoveryProfile ResolveEnergyRecoveryProfile()
    {
        if (pet == null)
        {
            pet = GetComponent<ThePet>();
        }

        EntityState<ThePet> currentState = pet?.states?.current;
        if (ThePetEnergyRecoveryProfileResolver.TryResolveFromState(currentState, out ThePetEnergyRecoveryProfile directProfile))
        {
            cachedRecoveryProfile = directProfile;
            return directProfile;
        }

        if (ThePetEnergyRecoveryProfileResolver.IsTemporaryState(currentState))
        {
            if (cachedRecoveryProfile.HasValue)
            {
                return cachedRecoveryProfile.Value;
            }

            EntityState<ThePet> lastState = pet?.states?.last;
            if (ThePetEnergyRecoveryProfileResolver.TryResolveFromState(lastState, out ThePetEnergyRecoveryProfile lastProfile))
            {
                cachedRecoveryProfile = lastProfile;
                return lastProfile;
            }
        }

        ThePetEnergyRecoveryProfile inferredProfile = ThePetEnergyRecoveryProfileResolver.InferFromStats(current_stats);
        cachedRecoveryProfile = inferredProfile;
        return inferredProfile;
    }

    private void OnValidate()
    {
        happinessDecaySettings ??= new ThePetHappinessDecaySettings();
        happinessDecaySettings.Sanitize();

        energyRecoverySettings ??= new ThePetEnergyRecoverySettings();
        energyRecoverySettings.Sanitize();

        satietyDecaySettings ??= new ThePetSatietyDecaySettings();
        satietyDecaySettings.Sanitize();

        autoSaveSettings ??= new AutoSaveSettings();
        autoSaveSettings.Sanitize();
    }
}
