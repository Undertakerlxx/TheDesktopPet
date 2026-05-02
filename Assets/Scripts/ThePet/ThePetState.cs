using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThePetState : EntityState<ThePet>
{
    protected const float HungryThreshold = 50f;
    protected const float SleepThreshold = 50f;
    protected const float HappyThreshold = 70f;
    protected const float LyingThreshold = 50f;
    protected const float MoodStateDelaySeconds = 30f;
    protected const float StretchDurationSeconds = 1.1f;

    protected static bool TryEnterPriorityNeedState(ThePet thePet)
    {
        return TryEnterHungryState(thePet) || TryEnterSleepState(thePet);
    }

    protected static bool TryEnterDragState(ThePet thePet)
    {
        if (thePet.inputs != null && thePet.inputs.GetDrag())
        {
            thePet.states.Change<DragState>();
            return true;
        }

        return false;
    }

    protected static bool TryEnterHungryState(ThePet thePet)
    {
        if (GetCurrentSatiety(thePet) < HungryThreshold)
        {
            if (!IsCurrentState<HungryState>(thePet))
            {
                thePet.states.Change<HungryState>();
            }

            return true;
        }

        return false;
    }

    protected static bool TryEnterSleepState(ThePet thePet)
    {
        if (GetCurrentSatiety(thePet) >= HungryThreshold
            && GetCurrentEnergy(thePet) < SleepThreshold)
        {
            if (!IsCurrentState<SleepState>(thePet))
            {
                thePet.states.Change<SleepState>();
            }

            return true;
        }

        return false;
    }

    protected static bool TryReturnToIdleOnRecentInteraction(ThePet thePet)
    {
        if (thePet.inputs != null && thePet.inputs.HasRecentInteraction())
        {
            thePet.states.Change<IdleState>();
            return true;
        }

        return false;
    }

    protected static bool HasBeenInactiveLongEnough(ThePet thePet)
    {
        return thePet.inputs != null
            && thePet.inputs.GetSecondsSinceInteraction() >= MoodStateDelaySeconds;
    }

    protected static void ChangeToMoodState(ThePet thePet)
    {
        float happiness = GetCurrentHappiness(thePet);
        if (happiness > HappyThreshold)
        {
            thePet.states.Change<HappyState>();
        }
        else if (happiness >= LyingThreshold)
        {
            thePet.states.Change<LyingState>();
        }
        else
        {
            thePet.states.Change<SadState>();
        }
    }

    protected static float GetCurrentHappiness(ThePet thePet)
    {
        if (thePet.statsManager == null || thePet.statsManager.current_stats == null)
        {
            return LyingThreshold;
        }

        return thePet.statsManager.current_stats.happiness;
    }

    protected static float GetCurrentSatiety(ThePet thePet)
    {
        if (thePet.statsManager == null || thePet.statsManager.current_stats == null)
        {
            return 100f;
        }

        return thePet.statsManager.current_stats.satiety;
    }

    protected static float GetCurrentEnergy(ThePet thePet)
    {
        if (thePet.statsManager == null || thePet.statsManager.current_stats == null)
        {
            return 100f;
        }

        return thePet.statsManager.current_stats.energy;
    }

    protected static void ChangeToIdleOrNeedState(ThePet thePet)
    {
        if (!TryEnterPriorityNeedState(thePet))
        {
            thePet.states.Change<IdleState>();
        }
    }

    private static bool IsCurrentState<TState>(ThePet thePet) where TState : ThePetState
    {
        return thePet != null
            && thePet.states != null
            && thePet.states.current != null
            && thePet.states.current.GetType() == typeof(TState);
    }
}
