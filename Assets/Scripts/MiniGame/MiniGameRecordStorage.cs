using System;
using System.IO;
using DesktopPet.Accounts;
using UnityEngine;

namespace DesktopPet.MiniGame
{
    [Serializable]
    public class MiniGameRecordsData
    {
        public float schulteBestTime;
        public int colorGridBestScore;
        public int eyeHandSpeedBestScore;
        public int geometryBestScore;
        public float dinoRunBestDistance;
        public float dodgeBallBestSurvival;

        public void Normalize()
        {
            schulteBestTime = Mathf.Max(0f, schulteBestTime);
            colorGridBestScore = Mathf.Max(0, colorGridBestScore);
            eyeHandSpeedBestScore = Mathf.Max(0, eyeHandSpeedBestScore);
            geometryBestScore = Mathf.Max(0, geometryBestScore);
            dinoRunBestDistance = Mathf.Max(0f, dinoRunBestDistance);
            dodgeBallBestSurvival = Mathf.Max(0f, dodgeBallBestSurvival);
        }
    }

    public class MiniGameRecordStorage
    {
        private const string SchulteBestTimeKey = "MiniGame.SchulteGrid.BestTime";
        private const string ColorGridBestScoreKey = "MiniGame.ColorGrid.BestScore";
        private const string EyeHandSpeedBestScoreKey = "MiniGame.EyeHandSpeed.BestScore";
        private const string GeometryBestScoreKey = "MiniGame.GeometryAtAGlance.BestScore";
        private const string DinoRunBestDistanceKey = "MiniGame.DinoRun.BestDistance";
        private const string DodgeBallBestSurvivalKey = "MiniGame.DodgeBall.BestSurvival";

        private string RecordsPath => AccountPathProvider.GetMiniGameRecordsPath();

        public MiniGameRecordsData Load()
        {
            return LoadFromPath(RecordsPath);
        }

        public void Save(MiniGameRecordsData data)
        {
            SaveToPath(RecordsPath, data);
        }

        public float GetSchulteBestTime()
        {
            return Load().schulteBestTime;
        }

        public void SetSchulteBestTime(float bestTime)
        {
            MiniGameRecordsData data = Load();
            data.schulteBestTime = bestTime;
            Save(data);
        }

        public int GetColorGridBestScore()
        {
            return Load().colorGridBestScore;
        }

        public void SetColorGridBestScore(int bestScore)
        {
            MiniGameRecordsData data = Load();
            data.colorGridBestScore = bestScore;
            Save(data);
        }

        public int GetEyeHandSpeedBestScore()
        {
            return Load().eyeHandSpeedBestScore;
        }

        public void SetEyeHandSpeedBestScore(int bestScore)
        {
            MiniGameRecordsData data = Load();
            data.eyeHandSpeedBestScore = bestScore;
            Save(data);
        }

        public int GetGeometryBestScore()
        {
            return Load().geometryBestScore;
        }

        public void SetGeometryBestScore(int bestScore)
        {
            MiniGameRecordsData data = Load();
            data.geometryBestScore = bestScore;
            Save(data);
        }

        public float GetDinoRunBestDistance()
        {
            return Load().dinoRunBestDistance;
        }

        public void SetDinoRunBestDistance(float bestDistance)
        {
            MiniGameRecordsData data = Load();
            data.dinoRunBestDistance = bestDistance;
            Save(data);
        }

        public float GetDodgeBallBestSurvival()
        {
            return Load().dodgeBallBestSurvival;
        }

        public void SetDodgeBallBestSurvival(float bestSurvival)
        {
            MiniGameRecordsData data = Load();
            data.dodgeBallBestSurvival = bestSurvival;
            Save(data);
        }

        public static MiniGameRecordsData LoadFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return CreateDefaultData();
            }

            try
            {
                string json = File.ReadAllText(path);
                MiniGameRecordsData data = JsonUtility.FromJson<MiniGameRecordsData>(json) ?? CreateDefaultData();
                data.Normalize();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"MiniGameRecordStorage: failed to load records. {exception.Message}");
                return CreateDefaultData();
            }
        }

        public static void SaveToPath(string path, MiniGameRecordsData data)
        {
            data ??= CreateDefaultData();
            data.Normalize();

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"MiniGameRecordStorage: failed to save records. {exception.Message}");
            }
        }

        public static bool HasLegacyPlayerPrefsData()
        {
            return PlayerPrefs.HasKey(SchulteBestTimeKey) ||
                   PlayerPrefs.HasKey(ColorGridBestScoreKey) ||
                   PlayerPrefs.HasKey(EyeHandSpeedBestScoreKey) ||
                   PlayerPrefs.HasKey(GeometryBestScoreKey) ||
                   PlayerPrefs.HasKey(DinoRunBestDistanceKey) ||
                   PlayerPrefs.HasKey(DodgeBallBestSurvivalKey);
        }

        public static MiniGameRecordsData LoadLegacyPlayerPrefs()
        {
            MiniGameRecordsData data = new()
            {
                schulteBestTime = PlayerPrefs.GetFloat(SchulteBestTimeKey, 0f),
                colorGridBestScore = PlayerPrefs.GetInt(ColorGridBestScoreKey, 0),
                eyeHandSpeedBestScore = PlayerPrefs.GetInt(EyeHandSpeedBestScoreKey, 0),
                geometryBestScore = PlayerPrefs.GetInt(GeometryBestScoreKey, 0),
                dinoRunBestDistance = PlayerPrefs.GetFloat(DinoRunBestDistanceKey, 0f),
                dodgeBallBestSurvival = PlayerPrefs.GetFloat(DodgeBallBestSurvivalKey, 0f)
            };

            data.Normalize();
            return data;
        }

        private static MiniGameRecordsData CreateDefaultData()
        {
            MiniGameRecordsData data = new();
            data.Normalize();
            return data;
        }
    }
}
