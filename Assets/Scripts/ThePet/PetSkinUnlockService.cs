using DesktopPet.Achievements;
using DesktopPet.Progress;
using UnityEngine;

public class PetSkinUnlockService
{
    private const int CompanionRequiredIntimacy = 50;
    private const int FarmRequiredLevel = 3;
    private const int FarmRequiredHarvests = 10;
    private const int KitchenRequiredCooks = 5;
    private const int KitchenRequiredPreferredFeeds = 1;
    private const int MiniGameRequiredSuccesses = 5;
    private const int MiniGameRequiredKinds = 3;
    private const int AchievementRequiredClaimed = 8;
    private const float CatalogRequiredRatio = 0.5f;
    private const string MiniGamePlayedCounterPrefix = "MiniGamePlayed.";

    private readonly ThePetStatsManager statsManager;
    private readonly AchievementService achievementService;

    public PetSkinUnlockService(ThePetStatsManager statsManager, DesktopPetProgressService progressService = null)
    {
        this.statsManager = statsManager;
        DesktopPetProgressService resolvedProgressService = progressService ?? new DesktopPetProgressService();
        achievementService = new AchievementService(statsManager, resolvedProgressService);
    }

    public bool IsUnlocked(PetSkinLibrary.UnlockCondition condition)
    {
        AchievementEvaluationContext context = achievementService.BuildContext();
        return condition switch
        {
            PetSkinLibrary.UnlockCondition.None => true,
            PetSkinLibrary.UnlockCondition.Intimacy50 => GetIntimacy(context) >= CompanionRequiredIntimacy,
            PetSkinLibrary.UnlockCondition.FarmLevel3AndHarvest10 =>
                context.farmLevel >= FarmRequiredLevel &&
                context.GetCount(AchievementEventType.FarmHarvest) >= FarmRequiredHarvests,
            PetSkinLibrary.UnlockCondition.KitchenCook5AndPreferredFeed1 =>
                context.GetCount(AchievementEventType.KitchenCook) >= KitchenRequiredCooks &&
                context.GetCount(AchievementEventType.PreferredFeed) >= KitchenRequiredPreferredFeeds,
            PetSkinLibrary.UnlockCondition.MiniGameSuccess5AndPlayed3Kinds =>
                GetMiniGameSuccessCount(context) >= MiniGameRequiredSuccesses &&
                GetPlayedMiniGameKindCount(context) >= MiniGameRequiredKinds,
            PetSkinLibrary.UnlockCondition.AchievementClaimed8OrCatalog50Percent =>
                GetClaimedAchievementCount(context) >= AchievementRequiredClaimed ||
                context.CatalogRatio >= CatalogRequiredRatio,
            _ => false
        };
    }

    public string GetProgressText(PetSkinLibrary.UnlockCondition condition)
    {
        AchievementEvaluationContext context = achievementService.BuildContext();
        return condition switch
        {
            PetSkinLibrary.UnlockCondition.None => "已解锁",
            PetSkinLibrary.UnlockCondition.Intimacy50 =>
                $"亲密 {ClampInt(GetIntimacy(context), CompanionRequiredIntimacy)}/{CompanionRequiredIntimacy}",
            PetSkinLibrary.UnlockCondition.FarmLevel3AndHarvest10 =>
                $"Lv{Mathf.Min(context.farmLevel, FarmRequiredLevel)}/{FarmRequiredLevel} 收获 {ClampInt(context.GetCount(AchievementEventType.FarmHarvest), FarmRequiredHarvests)}/{FarmRequiredHarvests}",
            PetSkinLibrary.UnlockCondition.KitchenCook5AndPreferredFeed1 =>
                $"料理 {ClampInt(context.GetCount(AchievementEventType.KitchenCook), KitchenRequiredCooks)}/{KitchenRequiredCooks} 偏好 {ClampInt(context.GetCount(AchievementEventType.PreferredFeed), KitchenRequiredPreferredFeeds)}/{KitchenRequiredPreferredFeeds}",
            PetSkinLibrary.UnlockCondition.MiniGameSuccess5AndPlayed3Kinds =>
                $"成功 {ClampInt(GetMiniGameSuccessCount(context), MiniGameRequiredSuccesses)}/{MiniGameRequiredSuccesses} 种类 {ClampInt(GetPlayedMiniGameKindCount(context), MiniGameRequiredKinds)}/{MiniGameRequiredKinds}",
            PetSkinLibrary.UnlockCondition.AchievementClaimed8OrCatalog50Percent =>
                $"成就 {ClampInt(GetClaimedAchievementCount(context), AchievementRequiredClaimed)}/{AchievementRequiredClaimed} 或图鉴 {Mathf.Min(context.CatalogRatio, CatalogRequiredRatio) * 100f:0}%",
            _ => string.Empty
        };
    }

    private static int GetMiniGameSuccessCount(AchievementEvaluationContext context)
    {
        return context.GetCount(AchievementEventType.FocusGameSuccess) +
               context.GetCount(AchievementEventType.ReactionGameSuccess) +
               context.GetCount(AchievementEventType.MovementGameSuccess);
    }

    private static int GetPlayedMiniGameKindCount(AchievementEvaluationContext context)
    {
        if (context.progressData == null)
        {
            return 0;
        }

        context.progressData.EnsureCollections();
        int count = 0;
        foreach (AchievementCounter counter in context.progressData.counters)
        {
            if (counter != null &&
                counter.count > 0 &&
                !string.IsNullOrEmpty(counter.key) &&
                counter.key.StartsWith(MiniGamePlayedCounterPrefix))
            {
                count++;
            }
        }

        return count;
    }

    private static int GetClaimedAchievementCount(AchievementEvaluationContext context)
    {
        if (context.progressData == null)
        {
            return 0;
        }

        context.progressData.EnsureCollections();
        int count = 0;
        foreach (AchievementProgressEntry entry in context.progressData.entries)
        {
            if (entry != null && entry.claimed)
            {
                count++;
            }
        }

        return count;
    }

    private float GetIntimacy(AchievementEvaluationContext context)
    {
        if (context.stats != null)
        {
            return context.stats.intimacy;
        }

        return statsManager != null && statsManager.current_stats != null ? statsManager.current_stats.intimacy : 0f;
    }

    private static int ClampInt(float current, int required)
    {
        return Mathf.Min(Mathf.FloorToInt(current), required);
    }
}
