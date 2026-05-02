using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : ThePetState
{
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
        if (TryEnterDragState(thePet))
        {
            return;
        }

        if (TryEnterPriorityNeedState(thePet))
        {
            return;
        }

        if (HasBeenInactiveLongEnough(thePet))
        {
            ChangeToMoodState(thePet);
        }
    }
}
