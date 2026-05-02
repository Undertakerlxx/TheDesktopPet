using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThePetInputManager : MonoBehaviour
{
    public event Action PetClicked;

    public InputActionAsset actions;
    public ThePet pet;

    protected InputAction m_drag;
    protected InputAction m_point;
    protected InputAction m_rightclick;
    protected bool m_dragStartedOnPet;
    protected bool m_pressStartedOnPet;
    protected Vector2 m_pressStartPointerPosition;
    protected float m_pressStartTime;
    protected float m_lastInteractionTime;

    private const float ClickMaxDuration = 0.25f;
    private const float ClickMaxMovement = 10f;
    private const float DragStartMovement = 12f;

    protected virtual void Awake()
    {
        pet = GetComponent<ThePet>();
        CacheActions();
        m_lastInteractionTime = Time.time;
    }

    protected virtual void OnEnable()
    {
        actions.Enable();
        m_drag.started += OnDragStarted;
        m_drag.canceled += OnDragCanceled;
        m_rightclick.started += OnRightClickStarted;
    }

    protected virtual void OnDisable()
    {
        m_drag.started -= OnDragStarted;
        m_drag.canceled -= OnDragCanceled;
        m_rightclick.started -= OnRightClickStarted;
        actions.Disable();
    }

    protected virtual void CacheActions()
    {
        m_drag = actions["LeftClick"];
        m_rightclick = actions["RightClick"];
        m_point = actions["Point"];

    }

    protected virtual void OnDragStarted(InputAction.CallbackContext context)
    {
        if (pet == null || pet.cam == null || pet.entityCollider == null)
        {
            m_dragStartedOnPet = false;
            m_pressStartedOnPet = false;
            return;
        }

        bool pointerDownOnPet = IsPointerDownOnPet(pet.cam, pet.entityCollider);
        m_dragStartedOnPet = pointerDownOnPet;
        m_pressStartedOnPet = pointerDownOnPet;

        if (pointerDownOnPet)
        {
            m_pressStartPointerPosition = GetPointerPosition();
            m_pressStartTime = Time.time;
            NotifyInteraction();
        }
    }

    protected virtual void OnDragCanceled(InputAction.CallbackContext context)
    {
        if (m_pressStartedOnPet && IsSimpleClick())
        {
            NotifyInteraction();
            PetClicked?.Invoke();
        }

        m_dragStartedOnPet = false;
        m_pressStartedOnPet = false;
    }

    protected virtual void OnRightClickStarted(InputAction.CallbackContext context)
    {
        if (pet == null || pet.cam == null || pet.entityCollider == null)
        {
            return;
        }

        if (IsPointerDownOnPet(pet.cam, pet.entityCollider))
        {
            NotifyInteraction();
        }
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
        return m_dragStartedOnPet && IsPointerPressed() && HasMovedEnoughForDrag();
    }

    public virtual bool IsSimpleClick()
    {
        if (!m_pressStartedOnPet)
        {
            return false;
        }

        float clickDuration = Time.time - m_pressStartTime;
        float clickMovement = Vector2.Distance(m_pressStartPointerPosition, GetPointerPosition());
        return clickDuration <= ClickMaxDuration && clickMovement <= ClickMaxMovement;
    }

    public virtual bool HasMovedEnoughForDrag()
    {
        if (!m_pressStartedOnPet)
        {
            return false;
        }

        float dragMovement = Vector2.Distance(m_pressStartPointerPosition, GetPointerPosition());
        return dragMovement >= DragStartMovement;
    }

    public virtual void NotifyInteraction()
    {
        m_lastInteractionTime = Time.time;
    }

    public virtual float GetSecondsSinceInteraction()
    {
        return Time.time - m_lastInteractionTime;
    }

    public virtual bool HasRecentInteraction(float threshold = 0.2f)
    {
        return GetSecondsSinceInteraction() <= threshold;
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
