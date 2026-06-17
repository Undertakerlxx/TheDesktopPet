using System.Collections;
using System.Collections.Generic;
using DesktopPet.AICompanion;
using UnityEngine;

public class DragState : ThePetState
{
    protected override void OnEnter(ThePet thePet)
    {
        if (thePet.inputs == null || thePet.windowController == null)
        {
            return;
        }

        thePet.inputs.NotifyInteraction();
        thePet.dragStartPointerPosition = thePet.inputs.GetDesktopPointerPosition();
        thePet.dragStartWindowPosition = thePet.windowController.GetWindowPosition();
    }

    protected override void OnExit(ThePet thePet)
    {
        AICompanionDialogueController.NotifyEvent(AICompanionEventType.PetDragged);
    }

    protected override void OnStep(ThePet thePet)
    {
        if (thePet.inputs != null && thePet.windowController != null)
        {
            thePet.inputs.NotifyInteraction();
            Vector2Int currentPointerPosition = thePet.inputs.GetDesktopPointerPosition();
            Vector2Int pointerDelta = currentPointerPosition - thePet.dragStartPointerPosition;

            Vector2Int nextWindowPosition = thePet.dragStartWindowPosition + pointerDelta;

            thePet.windowController.SetWindowPosition(nextWindowPosition);
        }
        
        if (thePet.inputs == null || !thePet.inputs.IsPointerPressed())
        {
            ChangeToIdleOrNeedState(thePet);
        }
    }
}
