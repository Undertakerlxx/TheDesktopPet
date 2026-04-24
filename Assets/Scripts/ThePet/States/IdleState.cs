using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IdleState : ThePetState
{
    private const float LyingDelaySeconds = 30f;

    protected override void OnEnter(ThePet thePet)
    {
        if (thePet.animatorComponent != null)
        {
            thePet.animatorComponent.Play("Idle", 0, 0f);
        }
    }

    protected override void OnExit(ThePet thePet)
    {
        
    }

    protected override void OnStep(ThePet thePet)
    {
        if (thePet.inputs.GetDrag())
        {
            thePet.states.Change<DragState>();
            return;
        }

        if (thePet.inputs.GetSecondsSinceInteraction() >= LyingDelaySeconds)
        {
            float happiness = GetCurrentHappiness(thePet);
            if (happiness > 70f)
            {
                thePet.states.Change<HappyState>();
            }
            else if (happiness >= 50f)
            {
                thePet.states.Change<LyingState>();
            }
            else
            {
                thePet.states.Change<SadState>();
            }
        }
    }

    private static float GetCurrentHappiness(ThePet thePet)
    {
        if (thePet.statsManager == null || thePet.statsManager.current_stats == null)
        {
            return 50f;
        }

        return thePet.statsManager.current_stats.happiness;
    }
}
