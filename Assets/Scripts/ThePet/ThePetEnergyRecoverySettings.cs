using System;
using UnityEngine;

[Serializable]
public class ThePetEnergyRecoverySettings
{
    [Min(0f)] public float idleRecoveryPerMinute = 5f;
    [Min(0f)] public float happyRecoveryPerMinute = 5f;
    [Min(0f)] public float sadRecoveryPerMinute = 2f;
    [Min(0f)] public float hungryRecoveryPerMinute = 2f;
    [Min(0f)] public float sleepRecoveryPerMinute = 8f;

    public void Sanitize()
    {
        idleRecoveryPerMinute = Mathf.Max(0f, idleRecoveryPerMinute);
        happyRecoveryPerMinute = Mathf.Max(0f, happyRecoveryPerMinute);
        sadRecoveryPerMinute = Mathf.Max(0f, sadRecoveryPerMinute);
        hungryRecoveryPerMinute = Mathf.Max(0f, hungryRecoveryPerMinute);
        sleepRecoveryPerMinute = Mathf.Max(0f, sleepRecoveryPerMinute);
    }

    public float GetRecoveryPerMinute(ThePetEnergyRecoveryProfile profile)
    {
        switch (profile)
        {
            case ThePetEnergyRecoveryProfile.Happy:
                return happyRecoveryPerMinute;
            case ThePetEnergyRecoveryProfile.Sad:
                return sadRecoveryPerMinute;
            case ThePetEnergyRecoveryProfile.Hungry:
                return hungryRecoveryPerMinute;
            case ThePetEnergyRecoveryProfile.Sleep:
                return sleepRecoveryPerMinute;
            default:
                return idleRecoveryPerMinute;
        }
    }
}
