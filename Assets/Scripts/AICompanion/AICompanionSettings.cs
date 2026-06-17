using UnityEngine;

namespace DesktopPet.AICompanion
{
    [System.Serializable]
    public class AICompanionSettings
    {
        public bool enableCompanionDialogue = true;
        public bool allowAiGeneration = true;
        public bool allowPetStateInPrompt = true;
        public bool fallbackOnly;
        public bool recordToMemory = true;

        [Range(0f, 1f)] public float petClickAiProbability = 1f;
        [Range(0f, 1f)] public float petDragAiProbability = 1f;

        [Min(0f)] public float hungryThreshold = 30f;
        [Min(0f)] public float lowEnergyThreshold = 40f;
        [Min(30f)] public float idleGreetingSeconds = 600f;
        [Min(1f)] public float statusCheckIntervalSeconds = 5f;
        [Min(0f)] public float startupGreetingDelaySeconds = 4f;
        [Min(0.5f)] public float softTimeoutSeconds = 2f;
        [Min(1f)] public float hardTimeoutSeconds = 5f;
        [Min(1)] public int maxAiRequestsPerHour = 8;
        [Min(8)] public int maxReplyCharacters = 30;

        public float GetCooldownSeconds(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.PetClicked:
                    return 45f;
                case AICompanionEventType.PetDragged:
                    return 60f;
                case AICompanionEventType.HungryWarning:
                case AICompanionEventType.LowEnergyWarning:
                    return 300f;
                case AICompanionEventType.IdleGreeting:
                    return 900f;
                case AICompanionEventType.MorningGreeting:
                case AICompanionEventType.NightGreeting:
                    return 3600f;
                default:
                    return 60f;
            }
        }

        public float GetTriggerProbability(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.PetClicked:
                    return petClickAiProbability;
                case AICompanionEventType.PetDragged:
                    return petDragAiProbability;
                default:
                    return 1f;
            }
        }

        public AICompanionPriority GetPriority(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.HungryWarning:
                case AICompanionEventType.LowEnergyWarning:
                    return AICompanionPriority.High;
                case AICompanionEventType.PetClicked:
                case AICompanionEventType.PetDragged:
                    return AICompanionPriority.Medium;
                default:
                    return AICompanionPriority.Low;
            }
        }

        public void Sanitize()
        {
            petClickAiProbability = Mathf.Clamp01(petClickAiProbability);
            petDragAiProbability = Mathf.Clamp01(petDragAiProbability);
            hungryThreshold = Mathf.Max(0f, hungryThreshold);
            lowEnergyThreshold = Mathf.Max(0f, lowEnergyThreshold);
            idleGreetingSeconds = Mathf.Max(30f, idleGreetingSeconds);
            statusCheckIntervalSeconds = Mathf.Max(1f, statusCheckIntervalSeconds);
            startupGreetingDelaySeconds = Mathf.Max(0f, startupGreetingDelaySeconds);
            softTimeoutSeconds = Mathf.Max(0.5f, softTimeoutSeconds);
            hardTimeoutSeconds = Mathf.Max(softTimeoutSeconds + 0.5f, hardTimeoutSeconds);
            maxAiRequestsPerHour = Mathf.Max(1, maxAiRequestsPerHour);
            maxReplyCharacters = Mathf.Max(8, maxReplyCharacters);
        }
    }
}
