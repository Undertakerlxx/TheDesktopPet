using System;

namespace DesktopPet.Save
{
    [Serializable]
    public class ThePetStatsSaveData
    {
        public float intimacy;
        public float happiness;
        public float energy;
        public float energy_max;
        public float focus;
        public float satiety;

        public static ThePetStatsSaveData FromStats(ThePetStats stats)
        {
            if (stats == null)
            {
                return null;
            }

            return new ThePetStatsSaveData
            {
                intimacy = stats.intimacy,
                happiness = stats.happiness,
                energy = stats.energy,
                energy_max = stats.energy_max,
                focus = stats.focus,
                satiety = stats.satiety,
            };
        }

        public void ApplyTo(ThePetStats stats)
        {
            if (stats == null)
            {
                return;
            }

            stats.intimacy = intimacy;
            stats.happiness = happiness;
            stats.energy = energy;
            stats.energy_max = energy_max;
            stats.focus = focus;
            stats.satiety = satiety;
        }
    }
}
