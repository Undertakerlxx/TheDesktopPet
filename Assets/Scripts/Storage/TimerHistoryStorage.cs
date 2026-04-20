using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DesktopPet.Storage
{
    [Serializable]
    public class TimerHistoryRecord
    {
        public string task;
        public float elapsedSeconds;
        public string finishedAt;
    }

    public class TimerHistoryStorage
    {
        private readonly string historyPath;

        public TimerHistoryStorage()
        {
            historyPath = Path.Combine(Application.persistentDataPath, "timer-history.json");
        }

        public List<TimerHistoryRecord> Load()
        {
            if (!File.Exists(historyPath))
            {
                return new List<TimerHistoryRecord>();
            }

            try
            {
                string json = File.ReadAllText(historyPath);
                TimerHistoryData data = JsonUtility.FromJson<TimerHistoryData>(json);
                return data?.records ?? new List<TimerHistoryRecord>();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TimerHistoryStorage: failed to load history. {exception.Message}");
                return new List<TimerHistoryRecord>();
            }
        }

        public void Save(List<TimerHistoryRecord> records)
        {
            try
            {
                string directory = Path.GetDirectoryName(historyPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                TimerHistoryData data = new TimerHistoryData { records = records };
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(historyPath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TimerHistoryStorage: failed to save history. {exception.Message}");
            }
        }

        [Serializable]
        private class TimerHistoryData
        {
            public List<TimerHistoryRecord> records = new List<TimerHistoryRecord>();
        }
    }
}
