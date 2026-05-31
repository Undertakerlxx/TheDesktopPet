using UnityEngine;

namespace DesktopPet.UI
{
    public static class GameSettingsStore
    {
        private const string TopmostKey = "GameSettings.Topmost";
        private const string FrameRateIndexKey = "GameSettings.FrameRateIndex";
        private const string PetScaleIndexKey = "GameSettings.PetScaleIndex";
        private const string StatsDisplayKey = "GameSettings.StatsDisplay";
        private const string EscapeQuitKey = "GameSettings.EscapeQuit";
        private const string MenuPositionIndexKey = "GameSettings.MenuPositionIndex";

        public static readonly string[] FrameRateLabels = { "60 FPS", "120 FPS", "144 FPS" };
        public static readonly int[] FrameRateValues = { 60, 120, 144 };
        public static readonly string[] PetScaleLabels = { "\u5c0f", "\u6807\u51c6", "\u5927" };
        public static readonly float[] PetScaleValues = { 0.85f, 1f, 1.15f };
        public static readonly string[] MenuPositionLabels = { "\u5c45\u5de6", "\u5c45\u4e2d", "\u5c45\u53f3" };
        public static readonly Vector2[] MenuAnchoredPositions =
        {
            new Vector2(-160f, 54f),
            new Vector2(0f, 54f),
            new Vector2(160f, 54f)
        };

        public static bool IsTopmostEnabled()
        {
            return PlayerPrefs.GetInt(TopmostKey, 1) != 0;
        }

        public static void SetTopmostEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(TopmostKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static int GetFrameRateIndex()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(FrameRateIndexKey, 2), 0, FrameRateLabels.Length - 1);
        }

        public static int GetTargetFrameRate()
        {
            return FrameRateValues[GetFrameRateIndex()];
        }

        public static void SetFrameRateIndex(int index)
        {
            PlayerPrefs.SetInt(FrameRateIndexKey, Mathf.Clamp(index, 0, FrameRateLabels.Length - 1));
            PlayerPrefs.Save();
        }

        public static int GetPetScaleIndex()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(PetScaleIndexKey, 1), 0, PetScaleLabels.Length - 1);
        }

        public static float GetPetScaleMultiplier()
        {
            return PetScaleValues[GetPetScaleIndex()];
        }

        public static void SetPetScaleIndex(int index)
        {
            PlayerPrefs.SetInt(PetScaleIndexKey, Mathf.Clamp(index, 0, PetScaleLabels.Length - 1));
            PlayerPrefs.Save();
        }

        public static bool IsStatsDisplayEnabled()
        {
            return PlayerPrefs.GetInt(StatsDisplayKey, 1) != 0;
        }

        public static void SetStatsDisplayEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(StatsDisplayKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool IsEscapeQuitEnabled()
        {
            return PlayerPrefs.GetInt(EscapeQuitKey, 1) != 0;
        }

        public static void SetEscapeQuitEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(EscapeQuitKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static int GetMenuPositionIndex()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(MenuPositionIndexKey, 1), 0, MenuPositionLabels.Length - 1);
        }

        public static Vector2 GetMenuAnchoredPosition()
        {
            return MenuAnchoredPositions[GetMenuPositionIndex()];
        }

        public static void SetMenuPositionIndex(int index)
        {
            PlayerPrefs.SetInt(MenuPositionIndexKey, Mathf.Clamp(index, 0, MenuPositionLabels.Length - 1));
            PlayerPrefs.Save();
        }

        public static void ResetToDefault()
        {
            SetTopmostEnabled(true);
            SetFrameRateIndex(2);
            SetPetScaleIndex(1);
            SetStatsDisplayEnabled(true);
            SetEscapeQuitEnabled(true);
            SetMenuPositionIndex(1);
        }
    }
}
