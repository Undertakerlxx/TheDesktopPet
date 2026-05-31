using System;
using System.IO;
using UnityEngine;

namespace DesktopPet.Achievements
{
    public static class AchievementEventRecorder
    {
        private const string SaveFileName = "achievement-progress.json";

        public static void Record(AchievementEventType eventType, int count = 1, float amount = 0f)
        {
            Record(eventType.ToString(), count, amount);
        }

        public static void Record(string key, int count = 1, float amount = 0f)
        {
            if (count <= 0 && amount <= 0f)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            try
            {
                AchievementProgressData data = Load();
                AchievementCounter counter = data.GetOrCreateCounter(key);
                counter.count += Math.Max(0, count);
                counter.totalAmount += Math.Max(0f, amount);
                Save(data);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AchievementEventRecorder: failed to record {key}. {exception.Message}");
            }
        }

        internal static AchievementProgressData Load()
        {
            string path = GetSavePath();
            if (!File.Exists(path))
            {
                return new AchievementProgressData();
            }

            try
            {
                string json = File.ReadAllText(path);
                AchievementProgressData data = JsonUtility.FromJson<AchievementProgressData>(json);
                data ??= new AchievementProgressData();
                data.EnsureCollections();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AchievementEventRecorder: failed to load progress. {exception.Message}");
                return new AchievementProgressData();
            }
        }

        internal static void Save(AchievementProgressData data)
        {
            string path = GetSavePath();
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            data ??= new AchievementProgressData();
            data.EnsureCollections();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }
    }
}
