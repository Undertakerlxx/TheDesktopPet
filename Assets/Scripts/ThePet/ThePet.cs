using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePet : Entity<ThePet>
{
    public ThePetInputManager inputs;
    protected virtual void InitializeInputs()
    {
        inputs = GetComponent<ThePetInputManager>();
    }

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
    }
}
