using System;
using UnityEngine;

[Serializable]
public class ThePetHappinessDecaySettings
{
    [Min(0f)] public float baseDecayPerMinute = 0.5f;
    [Min(0f)] public float highSatietyDecayPerMinute = 0.3f;
    [Min(0f)] public float mediumSatietyDecayPerMinute = 0.5f;
    [Min(0f)] public float lowSatietyDecayPerMinute = 0.8f;
    [Min(0f)] public float criticalSatietyDecayPerMinute = 1.2f;

    [Range(0f, 100f)] public float highSatietyThreshold = 80f;
    [Range(0f, 100f)] public float mediumSatietyThreshold = 50f;
    [Range(0f, 100f)] public float lowSatietyThreshold = 30f;

    public void Sanitize()
    {
        highSatietyThreshold = Mathf.Clamp(highSatietyThreshold, 0f, 100f);
        mediumSatietyThreshold = Mathf.Clamp(mediumSatietyThreshold, 0f, highSatietyThreshold);
        lowSatietyThreshold = Mathf.Clamp(lowSatietyThreshold, 0f, mediumSatietyThreshold);

        baseDecayPerMinute = Mathf.Max(0f, baseDecayPerMinute);
        highSatietyDecayPerMinute = Mathf.Max(0f, highSatietyDecayPerMinute);
        mediumSatietyDecayPerMinute = Mathf.Max(0f, mediumSatietyDecayPerMinute);
        lowSatietyDecayPerMinute = Mathf.Max(0f, lowSatietyDecayPerMinute);
        criticalSatietyDecayPerMinute = Mathf.Max(0f, criticalSatietyDecayPerMinute);
    }

    public float GetTotalDecayPerMinute(float satiety)
    {
        return baseDecayPerMinute + GetSatietyDecayPerMinute(satiety);
    }

    public float GetSatietyDecayPerMinute(float satiety)
    {
        if (satiety > highSatietyThreshold)
        {
            return highSatietyDecayPerMinute;
        }

        if (satiety >= mediumSatietyThreshold)
        {
            return mediumSatietyDecayPerMinute;
        }

        if (satiety >= lowSatietyThreshold)
        {
            return lowSatietyDecayPerMinute;
        }

        return criticalSatietyDecayPerMinute;
    }
}
