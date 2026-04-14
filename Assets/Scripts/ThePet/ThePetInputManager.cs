using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThePetInputManager : MonoBehaviour
{
    public InputActionAsset actions;

    protected InputAction m_drag;

    protected virtual void Awake()
    {
        CacheActions();
    }

    protected virtual void Start()
    {
        actions.Enable();
    }

    protected virtual void CacheActions()
    {
        m_drag = actions["LeftClick"];
    }

    public virtual bool GetDrag() => m_drag.IsPressed();
}
