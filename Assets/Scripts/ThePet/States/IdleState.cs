using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IdleState : ThePetState
{
    protected override void OnEnter(ThePet thePet)
    {
        
    }

    protected override void OnExit(ThePet thePet)
    {
        
    }

    protected override void OnStep(ThePet thePet)
    {
        if(thePet.inputs.GetDrag())
        {
            thePet.states.Change<DragState>();
        }
    }
}
