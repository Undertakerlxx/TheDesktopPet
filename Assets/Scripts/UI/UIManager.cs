using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DesktopPet.UI
{
    public class UIManager : MonoBehaviour
    {
        public Camera petCamera;
        public Collider2D petCollider;
        public RectTransform mainPanelTransform;
        public UIPanelLayer mainPanelLayer;
        public DesktopMenuPanelController mainPanelController;
        public UIWindowController[] windows;
        public Vector2 panelAnchoredPosition = new Vector2(0f, 54f);

        private readonly Dictionary<UIWindowType, UIWindowController> windowLookup = new();

        private void Awake()
        {
            InitializePanel();
            InitializeWindows();
            HideAllWindows();
            HideMainPanel();
        }

        private void Start()
        {
            AutoWirePetReferences();
        }

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame && IsPointerOnPet())
            {
                OpenMainPanel();
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame &&
                mainPanelLayer != null &&
                mainPanelLayer.IsVisible &&
                !IsPointerOverUi())
            {
                HideMainPanel();
            }
        }

        public void OpenMainPanel()
        {
            if (mainPanelTransform != null)
            {
                mainPanelTransform.anchoredPosition = panelAnchoredPosition;
            }

            if (mainPanelLayer != null)
            {
                mainPanelLayer.Show();
            }
        }

        public void HideMainPanel()
        {
            if (mainPanelLayer != null)
            {
                mainPanelLayer.Hide();
            }
        }

        public void OpenWindow(UIWindowType windowType)
        {
            HideMainPanel();
            HideAllWindows();

            if (windowLookup.TryGetValue(windowType, out UIWindowController controller))
            {
                controller.Open();
            }
        }

        public void CloseWindow(UIWindowType windowType)
        {
            if (windowLookup.TryGetValue(windowType, out UIWindowController controller))
            {
                controller.Close();
            }
        }

        public void HideAllWindows()
        {
            foreach (UIWindowController controller in windowLookup.Values)
            {
                controller.Close();
            }
        }

        private void InitializePanel()
        {
            if (mainPanelLayer == null)
            {
                return;
            }

            if (mainPanelTransform == null)
            {
                mainPanelTransform = mainPanelLayer.GetComponent<RectTransform>();
            }

            if (mainPanelController != null)
            {
                mainPanelController.Initialize(this, mainPanelLayer);
            }
        }

        private void InitializeWindows()
        {
            windowLookup.Clear();

            if (windows == null)
            {
                return;
            }

            foreach (UIWindowController controller in windows)
            {
                if (controller == null)
                {
                    continue;
                }

                controller.Initialize(this);
                windowLookup[controller.windowType] = controller;
            }
        }

        private void AutoWirePetReferences()
        {
            if (petCamera != null && petCollider != null)
            {
                return;
            }

            ThePet pet = FindFirstObjectByType<ThePet>();
            if (pet == null)
            {
                return;
            }

            if (petCamera == null)
            {
                petCamera = pet.cam;
            }

            if (petCollider == null)
            {
                petCollider = pet.entityCollider;
            }
        }

        private bool IsPointerOnPet()
        {
            AutoWirePetReferences();

            if (petCamera == null || petCollider == null || Mouse.current == null)
            {
                return false;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = petCamera.ScreenToWorldPoint(screenPosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
            return hitCollider == petCollider;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
