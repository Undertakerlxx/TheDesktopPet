using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.AICompanion
{
    public class AICompanionMemoryStore
    {
        private const string MorningGreetingDateKey = "AICompanion.MorningGreetingDate";
        private const string NightGreetingDateKey = "AICompanion.NightGreetingDate";
        private const int MaxRecentLines = 3;

        private readonly List<string> recentLines = new List<string>();
        private string currentHourKey;
        private int aiRequestsThisHour;

        public string LastLine => recentLines.Count > 0 ? recentLines[recentLines.Count - 1] : string.Empty;

        public bool CanUseHourlyAi(int maxRequestsPerHour)
        {
            RefreshHour();
            return aiRequestsThisHour < maxRequestsPerHour;
        }

        public void RecordAiRequest()
        {
            RefreshHour();
            aiRequestsThisHour++;
        }

        public void RecordDialogue(AICompanionEventType eventType, string line, bool fromAi, bool recordDailyGreeting = true)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                recentLines.Add(line);
                while (recentLines.Count > MaxRecentLines)
                {
                    recentLines.RemoveAt(0);
                }
            }

            if (recordDailyGreeting && IsDailyGreeting(eventType))
            {
                MarkDailyGreetingShown(eventType);
            }
        }

        public bool WasDailyGreetingShown(AICompanionEventType eventType)
        {
            string key = GetDailyGreetingKey(eventType);
            return !string.IsNullOrEmpty(key)
                && PlayerPrefs.GetString(key, string.Empty) == GetTodayKey();
        }

        private void MarkDailyGreetingShown(AICompanionEventType eventType)
        {
            string key = GetDailyGreetingKey(eventType);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetString(key, GetTodayKey());
            PlayerPrefs.Save();
        }

        private void RefreshHour()
        {
            string hourKey = DateTime.Now.ToString("yyyy-MM-dd HH");
            if (currentHourKey == hourKey)
            {
                return;
            }

            currentHourKey = hourKey;
            aiRequestsThisHour = 0;
        }

        private static bool IsDailyGreeting(AICompanionEventType eventType)
        {
            return eventType == AICompanionEventType.MorningGreeting
                || eventType == AICompanionEventType.NightGreeting;
        }

        private static string GetDailyGreetingKey(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.MorningGreeting:
                    return MorningGreetingDateKey;
                case AICompanionEventType.NightGreeting:
                    return NightGreetingDateKey;
                default:
                    return string.Empty;
            }
        }

        private static string GetTodayKey()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
