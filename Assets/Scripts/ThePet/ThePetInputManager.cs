using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThePetInputManager : MonoBehaviour
{
    public InputActionAsset actions;
    public ThePet pet;

    protected InputAction m_drag;
    protected InputAction m_point;
    protected bool m_dragStartedOnPet;

    protected virtual void Awake()
    {
        pet = GetComponent<ThePet>();
        CacheActions();
    }

    protected virtual void OnEnable()
    {
        actions.Enable();
        m_drag.started += OnDragStarted;
        m_drag.canceled += OnDragCanceled;
    }

    protected virtual void OnDisable()
    {
        m_drag.started -= OnDragStarted;
        m_drag.canceled -= OnDragCanceled;
        actions.Disable();
    }

    protected virtual void CacheActions()
    {
        m_drag = actions["LeftClick"];
        m_point = actions["Point"];
    }

    protected virtual void OnDragStarted(InputAction.CallbackContext context)
    {
        if (pet == null || pet.cam == null || pet.entityCollider == null)
        {
            m_dragStartedOnPet = false;
            return;
        }

        m_dragStartedOnPet = IsPointerDownOnPet(pet.cam, pet.entityCollider);
    }

    protected virtual void OnDragCanceled(InputAction.CallbackContext context)
    {
        m_dragStartedOnPet = false;
    }

    public bool IsPointerDownOnPet(Camera cam,Collider2D petCollider)
    {
        Vector2 screenPos = m_point.ReadValue<Vector2>();
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 point = new Vector2(worldPos.x, worldPos.y);

        return petCollider == Physics2D.OverlapPoint(point);


    }

    public virtual bool GetDrag()
    {
        return m_dragStartedOnPet && m_drag.IsPressed();
    }

    public virtual bool IsPointerPressed()
    {
        return m_drag != null && m_drag.IsPressed();
    }

    public virtual Vector2 GetPointerPosition()
    {
        return m_point.ReadValue<Vector2>();
    }

    public virtual Vector2Int GetDesktopPointerPosition()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (GetCursorPos(out POINT point))
        {
            return new Vector2Int(point.x, point.y);
        }
#endif

        Vector2 pointerPosition = GetPointerPosition();
        return new Vector2Int(
            Mathf.RoundToInt(pointerPosition.x),
            Mathf.RoundToInt(pointerPosition.y));
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }
    
}
