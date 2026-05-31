using System;
using System.Collections.Generic;
using DesktopPet.Catalog;
using DesktopPet.Progress;
using DesktopPet.Storage;
using UnityEngine;

namespace DesktopPet.Achievements
{
    public class AchievementViewModel
    {
        public AchievementDefinition definition;
        public bool unlocked;
        public bool claimed;
        public string progressText;
    }

    public class AchievementService
    {
        private readonly DesktopPetProgressService progressService;
        private readonly TimerHistoryStorage timerHistoryStorage;
        private ThePetStatsManager statsManager;
        private AchievementProgressData progressData;

        public AchievementService(ThePetStatsManager statsManager = null, DesktopPetProgressService progressService = null)
        {
            this.statsManager = statsManager;
            this.progressService = progressService ?? new DesktopPetProgressService();
            timerHistoryStorage = new TimerHistoryStorage();
            progressData = AchievementEventRecorder.Load();
        }

        public List<AchievementViewModel> GetAchievements(AchievementCategory category)
        {
            AchievementEvaluationContext context = BuildContext();
            bool changed = RefreshUnlocks(context);
            List<AchievementViewModel> results = new();

            foreach (AchievementDefinition definition in AchievementDatabase.Definitions)
            {
                if (definition.category != category)
                {
                    continue;
                }

                AchievementProgressEntry entry = progressData.GetOrCreateEntry(definition.id);
                bool unlocked = !string.IsNullOrEmpty(entry.unlockedAt);
                results.Add(new AchievementViewModel
                {
                    definition = definition,
                    unlocked = unlocked,
                    claimed = entry.claimed,
                    progressText = definition.progressText(context)
                });
            }

            if (changed)
            {
                Save();
            }

            return results;
        }

        public AchievementEvaluationContext BuildContext()
        {
            progressData = AchievementEventRecorder.Load();
            progressData.EnsureCollections();
            progressService.Reload();

            if (statsManager == null)
            {
                statsManager = UnityEngine.Object.FindFirstObjectByType<ThePetStatsManager>();
            }

            List<TimerHistoryRecord> timerRecords = timerHistoryStorage.Load();
            float totalTimerSeconds = 0f;
            foreach (TimerHistoryRecord record in timerRecords)
            {
                if (record != null)
                {
                    totalTimerSeconds += Mathf.Max(0f, record.elapsedSeconds);
                }
            }

            DesktopPetProgressData progress = progressService.Data;
            return new AchievementEvaluationContext
            {
                stats = statsManager != null ? statsManager.current_stats : null,
                farmLevel = progress.FarmLevel,
                farmExperience = progress.farmExperience,
                kitchenExperience = progress.kitchenExperience,
                inventoryKinds = CountInventoryKinds(progress),
                dishKinds = CountDishKinds(progress),
                catalogUnlockedCount = progress.unlockedCatalogEntryIds != null ? progress.unlockedCatalogEntryIds.Count : 0,
                catalogTotalCount = CatalogDatabase.Entries.Count,
                timerRecordCount = timerRecords.Count,
                timerTotalSeconds = totalTimerSeconds,
                progressData = progressData
            };
        }

        public bool TryClaim(string achievementId, out string feedback)
        {
            feedback = "成就尚未达成";
            AchievementDefinition definition = FindDefinition(achievementId);
            if (definition == null)
            {
                feedback = "成就不存在";
                return false;
            }

            AchievementEvaluationContext context = BuildContext();
            RefreshUnlocks(context);

            AchievementProgressEntry entry = progressData.GetOrCreateEntry(achievementId);
            if (string.IsNullOrEmpty(entry.unlockedAt))
            {
                Save();
                return false;
            }

            if (entry.claimed)
            {
                feedback = "奖励已领取";
                return false;
            }

            ApplyReward(definition.reward);
            entry.claimed = true;
            entry.claimedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Save();

            feedback = BuildRewardText(definition.reward);
            return true;
        }

        public int GetUnlockedCount(AchievementCategory category)
        {
            AchievementEvaluationContext context = BuildContext();
            bool changed = RefreshUnlocks(context);
            if (changed)
            {
                Save();
            }

            return AchievementDatabase.GetCategoryCount(context, category);
        }

        public int GetClaimedCount(AchievementCategory category)
        {
            int count = 0;
            foreach (AchievementDefinition definition in AchievementDatabase.Definitions)
            {
                if (definition.category != category)
                {
                    continue;
                }

                AchievementProgressEntry entry = progressData.GetOrCreateEntry(definition.id);
                if (entry.claimed)
                {
                    count++;
                }
            }

            return count;
        }

        private bool RefreshUnlocks(AchievementEvaluationContext context)
        {
            bool changed = false;
            foreach (AchievementDefinition definition in AchievementDatabase.Definitions)
            {
                AchievementProgressEntry entry = progressData.GetOrCreateEntry(definition.id);
                if (!string.IsNullOrEmpty(entry.unlockedAt) || !definition.isUnlocked(context))
                {
                    continue;
                }

                entry.unlockedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                changed = true;
            }

            return changed;
        }

        private void ApplyReward(AchievementReward reward)
        {
            if (reward.IsEmpty)
            {
                return;
            }

            if (statsManager == null)
            {
                statsManager = UnityEngine.Object.FindFirstObjectByType<ThePetStatsManager>();
            }

            if (statsManager == null || statsManager.current_stats == null)
            {
                return;
            }

            ThePetStats stats = statsManager.current_stats;
            stats.intimacy += reward.intimacy;
            if (reward.energyMax > 0f)
            {
                stats.energy_max += reward.energyMax;
                stats.energy = Mathf.Min(stats.energy + reward.energyMax, stats.energy_max);
            }

            statsManager.NotifyStatsChanged();
            statsManager.SaveCurrentStats();
        }

        private void Save()
        {
            AchievementEventRecorder.Save(progressData);
        }

        private static AchievementDefinition FindDefinition(string achievementId)
        {
            foreach (AchievementDefinition definition in AchievementDatabase.Definitions)
            {
                if (definition.id == achievementId)
                {
                    return definition;
                }
            }

            return null;
        }

        private static int CountInventoryKinds(DesktopPetProgressData progress)
        {
            int count = 0;
            foreach (InventoryStack stack in progress.inventory)
            {
                if (stack != null && stack.amount > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountDishKinds(DesktopPetProgressData progress)
        {
            int count = 0;
            foreach (DishInventoryStack stack in progress.dishInventory)
            {
                if (stack != null && stack.amount > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static string BuildRewardText(AchievementReward reward)
        {
            if (reward.IsEmpty)
            {
                return "已领取";
            }

            List<string> parts = new();
            if (reward.intimacy != 0)
            {
                parts.Add($"亲密度 +{reward.intimacy}");
            }

            if (reward.energyMax > 0f)
            {
                parts.Add($"活力上限 +{reward.energyMax:0}");
            }

            return string.Join("  ", parts);
        }
    }
}
