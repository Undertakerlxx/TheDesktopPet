using System;

namespace DesktopPet.Progress
{
    [Serializable]
    public class FocusLevelDefinition
    {
        public string displayName;
        public int minFocus;
        public int maxFocus;
        public float durationMultiplier;
    }

    public static class ProgressionDatabase
    {
        private static readonly FocusLevelDefinition[] focusLevels =
        {
            new() { displayName = "入门", minFocus = 0, maxFocus = 99, durationMultiplier = 1f },
            new() { displayName = "熟练", minFocus = 100, maxFocus = 199, durationMultiplier = 0.9f },
            new() { displayName = "精通", minFocus = 200, maxFocus = 299, durationMultiplier = 0.75f },
            new() { displayName = "专家", minFocus = 300, maxFocus = 399, durationMultiplier = 0.6f },
            new() { displayName = "大师", minFocus = 400, maxFocus = 500, durationMultiplier = 0.5f }
        };

        public static FocusLevelDefinition[] FocusLevels => focusLevels;

        public static FocusLevelDefinition GetFocusLevel(float focus)
        {
            foreach (FocusLevelDefinition level in focusLevels)
            {
                if (focus >= level.minFocus && focus <= level.maxFocus)
                {
                    return level;
                }
            }

            return focus >= 500 ? focusLevels[^1] : focusLevels[0];
        }

        public static int ApplyFocusEfficiency(int baseMinutes, float focus)
        {
            FocusLevelDefinition focusLevel = GetFocusLevel(focus);
            return Math.Max(1, (int)Math.Ceiling(baseMinutes * focusLevel.durationMultiplier));
        }
    }
}
