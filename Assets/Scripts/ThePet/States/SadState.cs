using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SadState : ThePetState
{
    protected override void OnEnter(ThePet thePet)
    {
        if (thePet.animatorComponent != null)
        {
            thePet.animatorComponent.Play("Sad", 0, 0f);
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

        if (TryReturnToIdleOnRecentInteraction(thePet))
        {
            return;
        }
    }
}
