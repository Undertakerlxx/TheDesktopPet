using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePet : Entity<ThePet>
{
    public ThePetInputManager inputs;
    public WindowController windowController;
    public Vector2Int dragStartPointerPosition { get; set; }
    public Vector2Int dragStartWindowPosition { get; set; }

    protected virtual void InitializeInputs()
    {
        inputs = GetComponent<ThePetInputManager>();
    }

    protected virtual void InitializeWindowController()
    {
        windowController = FindObjectOfType<WindowController>();
    }

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeWindowController();
    }
}
