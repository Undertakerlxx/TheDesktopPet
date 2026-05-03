using System;
using UnityEngine;

[Serializable]
public class ThePetSatietyDecaySettings
{
    [Min(0f)] public float decayPerMinute = 5f;

    public void Sanitize()
    {
        decayPerMinute = Mathf.Max(0f, decayPerMinute);
    }
}
