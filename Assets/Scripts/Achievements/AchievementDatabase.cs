using System;
using System.Collections.Generic;
using DesktopPet.Catalog;
using DesktopPet.Kitchen;

namespace DesktopPet.Achievements
{
    public class AchievementDefinition
    {
        public string id;
        public string displayName;
        public AchievementCategory category;
        public string description;
        public string conditionText;
        public AchievementReward reward;
        public Func<AchievementEvaluationContext, bool> isUnlocked;
        public Func<AchievementEvaluationContext, string> progressText;
    }

    public class AchievementEvaluationContext
    {
        public ThePetStats stats;
        public int farmLevel;
        public int farmExperience;
        public int kitchenExperience;
        public int inventoryKinds;
        public int dishKinds;
        public int catalogUnlockedCount;
        public int catalogTotalCount;
        public int timerRecordCount;
        public float timerTotalSeconds;
        public AchievementProgressData progressData;

        public int GetCount(AchievementEventType eventType)
        {
            AchievementCounter counter = GetCounter(eventType);
            return counter != null ? counter.count : 0;
        }

        public float GetAmount(AchievementEventType eventType)
        {
            AchievementCounter counter = GetCounter(eventType);
            return counter != null ? counter.totalAmount : 0f;
        }

        public float CatalogRatio => catalogTotalCount <= 0 ? 0f : catalogUnlockedCount / (float)catalogTotalCount;

        private AchievementCounter GetCounter(AchievementEventType eventType)
        {
            if (progressData == null)
            {
                return null;
            }

            progressData.EnsureCollections();
            string key = eventType.ToString();
            foreach (AchievementCounter counter in progressData.counters)
            {
                if (counter.key == key)
                {
                    return counter;
                }
            }

            return null;
        }
    }

    public static class AchievementDatabase
    {
        private static readonly AchievementDefinition[] definitions =
        {
            Define("growth_first_meet", "初次相遇", AchievementCategory.Growth, "第一次打开桌宠的纪念。", "进入游戏", new AchievementReward(10),
                _ => true, _ => "已相遇"),

            Define("growth_first_feed", "第一次喂食", AchievementCategory.Growth, "给桌宠投喂第一份料理。", "成功喂食 1 次", new AchievementReward(15),
                c => c.GetCount(AchievementEventType.Feed) >= 1,
                c => CountText(c.GetCount(AchievementEventType.Feed), 1, "次")),

            Define("growth_close_partner", "亲密伙伴", AchievementCategory.Growth, "亲密度达到第一个阶段目标。", "亲密度达到 100", new AchievementReward(20),
                c => c.stats != null && c.stats.intimacy >= 100f,
                c => ValueText(c.stats != null ? c.stats.intimacy : 0f, 100f)),

            Define("growth_soul_partner", "灵魂伴侣", AchievementCategory.Growth, "长期陪伴后的最高亲密纪念。", "亲密度达到 1000", new AchievementReward(50),
                c => c.stats != null && c.stats.intimacy >= 1000f,
                c => ValueText(c.stats != null ? c.stats.intimacy : 0f, 1000f)),

            Define("growth_small_farmer", "小农场主", AchievementCategory.Growth, "农场等级提升到 2 级。", "农场等级达到 Lv2", new AchievementReward(15),
                c => c.farmLevel >= 2,
                c => $"Lv{c.farmLevel}/2"),

            Define("growth_farm_master", "农场大师", AchievementCategory.Growth, "农场经营进入成熟阶段。", "农场等级达到 Lv4", new AchievementReward(30),
                c => c.farmLevel >= 4,
                c => $"Lv{c.farmLevel}/4"),

            Define("growth_kitchen_beginner", "厨房新手", AchievementCategory.Growth, "完成第一道菜品制作。", "完成烹饪 1 次", new AchievementReward(15),
                c => c.GetCount(AchievementEventType.KitchenCook) >= 1 || c.kitchenExperience > 0,
                c => CountText(Math.Max(c.GetCount(AchievementEventType.KitchenCook), c.kitchenExperience > 0 ? 1 : 0), 1, "次")),

            Define("daily_time_keeper", "时间陪伴", AchievementCategory.Daily, "完成一次工作或学习计时。", "完成计时 1 次", new AchievementReward(10),
                c => c.timerRecordCount >= 1 || c.GetCount(AchievementEventType.TimerCompleted) >= 1,
                c => CountText(Math.Max(c.timerRecordCount, c.GetCount(AchievementEventType.TimerCompleted)), 1, "次")),

            Define("daily_one_hour", "一日陪伴", AchievementCategory.Daily, "累计专注陪伴达到一小时。", "累计计时 1 小时", new AchievementReward(20),
                c => GetTimerSeconds(c) >= 3600f,
                c => DurationText(GetTimerSeconds(c), 3600f)),

            Define("daily_touch_master", "摸头杀", AchievementCategory.Daily, "频繁互动带来的默契。", "点击互动 50 次", new AchievementReward(25),
                c => c.GetCount(AchievementEventType.Touch) >= 50,
                c => CountText(c.GetCount(AchievementEventType.Touch), 50, "次")),

            Define("challenge_focus_novice", "专注入门", AchievementCategory.Challenge, "完成一次专注类小游戏挑战。", "专注类小游戏成功 1 次", new AchievementReward(10),
                c => c.GetCount(AchievementEventType.FocusGameSuccess) >= 1,
                c => CountText(c.GetCount(AchievementEventType.FocusGameSuccess), 1, "次")),

            Define("challenge_reaction_novice", "反应新星", AchievementCategory.Challenge, "完成一次反应类小游戏挑战。", "反应类小游戏成功 1 次", new AchievementReward(10),
                c => c.GetCount(AchievementEventType.ReactionGameSuccess) >= 1,
                c => CountText(c.GetCount(AchievementEventType.ReactionGameSuccess), 1, "次")),

            Define("challenge_movement_novice", "运动健将", AchievementCategory.Challenge, "完成一次运动类小游戏挑战。", "运动类小游戏成功 1 次", new AchievementReward(10, 5f),
                c => c.GetCount(AchievementEventType.MovementGameSuccess) >= 1,
                c => CountText(c.GetCount(AchievementEventType.MovementGameSuccess), 1, "次")),

            Define("challenge_all_rounder", "游戏新手", AchievementCategory.Challenge, "尝试三种方向的小游戏。", "三类小游戏各成功 1 次", new AchievementReward(30),
                c => GetCompletedGameCategoryCount(c) >= 3,
                c => $"{GetCompletedGameCategoryCount(c)}/3 类"),

            Define("collection_first_harvest", "第一次收获", AchievementCategory.Collection, "从农场收获第一份作物。", "收获作物 1 次", new AchievementReward(10),
                c => c.GetCount(AchievementEventType.FarmHarvest) >= 1 || c.inventoryKinds > 0,
                c => CountText(Math.Max(c.GetCount(AchievementEventType.FarmHarvest), c.inventoryKinds > 0 ? 1 : 0), 1, "次")),

            Define("collection_catalog_open", "图鉴开启", AchievementCategory.Collection, "收录第一条图鉴内容。", "解锁图鉴 1 项", new AchievementReward(20),
                c => c.catalogUnlockedCount >= 1,
                c => CountText(c.catalogUnlockedCount, 1, "项")),

            Define("collection_catalog_half", "半本图鉴", AchievementCategory.Collection, "图鉴收集达到一半。", "图鉴收集达 50%", new AchievementReward(30),
                c => c.CatalogRatio >= 0.5f,
                c => PercentText(c.CatalogRatio)),

            Define("collection_foodie", "美食家", AchievementCategory.Collection, "拥有过多种不同菜品。", "收集 6 种菜品", new AchievementReward(30),
                c => c.dishKinds >= 6,
                c => CountText(c.dishKinds, 6, "种")),

            Define("collection_catalog_master", "收藏大师", AchievementCategory.Collection, "完成全部作物和菜品图鉴。", "图鉴全收集", new AchievementReward(60, 10f),
                c => c.catalogTotalCount > 0 && c.catalogUnlockedCount >= c.catalogTotalCount,
                c => $"{c.catalogUnlockedCount}/{c.catalogTotalCount}"),

            Define("hidden_perfect_care", "完美照顾", AchievementCategory.Hidden, "让桌宠维持在很棒的状态。", "开心、活力、饱食同时达到 80", new AchievementReward(40, 5f),
                c => c.stats != null && c.stats.happiness >= 80f && c.stats.energy >= 80f && c.stats.satiety >= 80f,
                c => c.stats == null ? "0/3 项" : $"{GetHighCareStatCount(c.stats)}/3 项")
        };

        public static IReadOnlyList<AchievementDefinition> Definitions => definitions;

        private static AchievementDefinition Define(
            string id,
            string displayName,
            AchievementCategory category,
            string description,
            string conditionText,
            AchievementReward reward,
            Func<AchievementEvaluationContext, bool> isUnlocked,
            Func<AchievementEvaluationContext, string> progressText)
        {
            return new AchievementDefinition
            {
                id = id,
                displayName = displayName,
                category = category,
                description = description,
                conditionText = conditionText,
                reward = reward,
                isUnlocked = isUnlocked,
                progressText = progressText
            };
        }

        public static string GetCategoryDisplayName(AchievementCategory category)
        {
            return category switch
            {
                AchievementCategory.Growth => "成长",
                AchievementCategory.Daily => "日常",
                AchievementCategory.Challenge => "挑战",
                AchievementCategory.Collection => "收集",
                AchievementCategory.Hidden => "隐藏",
                _ => category.ToString()
            };
        }

        public static int GetCategoryCount(AchievementEvaluationContext context, AchievementCategory category)
        {
            int count = 0;
            foreach (AchievementDefinition definition in definitions)
            {
                if (definition.category == category && definition.isUnlocked(context))
                {
                    count++;
                }
            }

            return count;
        }

        public static int GetCategoryTotal(AchievementCategory category)
        {
            int total = 0;
            foreach (AchievementDefinition definition in definitions)
            {
                if (definition.category == category)
                {
                    total++;
                }
            }

            return total;
        }

        private static string CountText(int current, int required, string unit)
        {
            return $"{Math.Min(current, required)}/{required} {unit}";
        }

        private static string ValueText(float current, float required)
        {
            return $"{Math.Min(current, required):0}/{required:0}";
        }

        private static string PercentText(float ratio)
        {
            return $"{Math.Min(1f, ratio) * 100f:0}%";
        }

        private static string DurationText(float currentSeconds, float requiredSeconds)
        {
            return $"{Math.Min(currentSeconds, requiredSeconds) / 60f:0}/{requiredSeconds / 60f:0} 分钟";
        }

        private static float GetTimerSeconds(AchievementEvaluationContext context)
        {
            return Math.Max(context.timerTotalSeconds, context.GetAmount(AchievementEventType.TimerCompleted));
        }

        private static int GetCompletedGameCategoryCount(AchievementEvaluationContext context)
        {
            int count = 0;
            if (context.GetCount(AchievementEventType.FocusGameSuccess) > 0)
            {
                count++;
            }

            if (context.GetCount(AchievementEventType.ReactionGameSuccess) > 0)
            {
                count++;
            }

            if (context.GetCount(AchievementEventType.MovementGameSuccess) > 0)
            {
                count++;
            }

            return count;
        }

        private static int GetHighCareStatCount(ThePetStats stats)
        {
            int count = 0;
            if (stats.happiness >= 80f)
            {
                count++;
            }

            if (stats.energy >= 80f)
            {
                count++;
            }

            if (stats.satiety >= 80f)
            {
                count++;
            }

            return count;
        }
    }
}
