using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ThePet))]
public class ThePetStateManager : EntityStateManager<ThePet>
{
    [ClassTypeName(typeof(ThePetState))]
    public string[] states;
    protected override List<EntityState<ThePet>> GetStateList()
    {
        return ThePetState.CreateStringFromArray(states);
    }
}
