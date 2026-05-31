using System;
using System.Collections.Generic;

namespace DesktopPet.Achievements
{
    public enum AchievementCategory
    {
        Growth,
        Daily,
        Challenge,
        Collection,
        Hidden
    }

    public enum AchievementEventType
    {
        Touch,
        Feed,
        PreferredFeed,
        MiniGamePlayed,
        FocusGameSuccess,
        ReactionGameSuccess,
        MovementGameSuccess,
        FarmHarvest,
        KitchenCook,
        TimerCompleted
    }

    [Serializable]
    public class AchievementCounter
    {
        public string key;
        public int count;
        public float totalAmount;
    }

    [Serializable]
    public class AchievementProgressEntry
    {
        public string achievementId;
        public bool claimed;
        public string unlockedAt;
        public string claimedAt;
    }

    [Serializable]
    public class AchievementProgressData
    {
        public List<AchievementCounter> counters = new();
        public List<AchievementProgressEntry> entries = new();

        public void EnsureCollections()
        {
            counters ??= new List<AchievementCounter>();
            entries ??= new List<AchievementProgressEntry>();
        }

        public AchievementCounter GetOrCreateCounter(string key)
        {
            EnsureCollections();
            foreach (AchievementCounter counter in counters)
            {
                if (counter.key == key)
                {
                    return counter;
                }
            }

            AchievementCounter created = new() { key = key };
            counters.Add(created);
            return created;
        }

        public AchievementProgressEntry GetOrCreateEntry(string achievementId)
        {
            EnsureCollections();
            foreach (AchievementProgressEntry entry in entries)
            {
                if (entry.achievementId == achievementId)
                {
                    return entry;
                }
            }

            AchievementProgressEntry created = new() { achievementId = achievementId };
            entries.Add(created);
            return created;
        }
    }

    public readonly struct AchievementReward
    {
        public readonly int intimacy;
        public readonly float energyMax;

        public AchievementReward(int intimacy = 0, float energyMax = 0f)
        {
            this.intimacy = intimacy;
            this.energyMax = energyMax;
        }

        public bool IsEmpty => intimacy == 0 && energyMax <= 0f;
    }
}
