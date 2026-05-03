using System;
using System.IO;
using UnityEngine;

namespace DesktopPet.Save
{
    public class GameSaveManager : Singleton<GameSaveManager>
    {
        private const string SaveDirectoryName = "Save";
        private const string StatsFileName = "the-pet-stats.json";

        private string statsSavePath;

        private string StatsSavePath
        {
            get
            {
                if (string.IsNullOrEmpty(statsSavePath))
                {
                    statsSavePath = Path.Combine(Application.persistentDataPath, SaveDirectoryName, StatsFileName);
                }

                return statsSavePath;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            _ = StatsSavePath;
        }

        public bool SaveStats(ThePetStats stats)
        {
            ThePetStatsSaveData data = ThePetStatsSaveData.FromStats(stats);
            if (data == null)
            {
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(StatsSavePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(StatsSavePath, json);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"GameSaveManager: failed to save pet stats. {exception.Message}");
                return false;
            }
        }

        public ThePetStatsSaveData LoadStats()
        {
            if (!File.Exists(StatsSavePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(StatsSavePath);
                return JsonUtility.FromJson<ThePetStatsSaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"GameSaveManager: failed to load pet stats. {exception.Message}");
                return null;
            }
        }
    }
}
