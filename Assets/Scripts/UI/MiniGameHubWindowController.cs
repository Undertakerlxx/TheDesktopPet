using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class MiniGameHubWindowController : UIWindowController
    {
        public Button reactionTabButton;
        public Button focusTabButton;
        public Button movementTabButton;
        public Button eyeHandSpeedButton;
        public Button geometryAtAGlanceButton;
        public Button schulteGridButton;
        public Button colorGridButton;
        public Button dinoRunButton;
        public Button dodgeBallButton;
        public Button closeButton;

        public GameObject reactionPage;
        public GameObject focusPage;
        public GameObject movementPage;

        public Image reactionTabImage;
        public Image focusTabImage;
        public Image movementTabImage;

        public Color selectedTabColor = new(0.95f, 0.86f, 0.72f, 0.95f);
        public Color normalTabColor = new(1f, 1f, 1f, 0.9f);

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            BindTabButton(reactionTabButton, ShowReactionPage);
            BindTabButton(focusTabButton, ShowFocusPage);
            BindTabButton(movementTabButton, ShowMovementPage);
            BindGameButton(eyeHandSpeedButton, UIWindowType.EyeHandSpeed);
            BindGameButton(geometryAtAGlanceButton, UIWindowType.GeometryAtAGlance);
            BindGameButton(schulteGridButton, UIWindowType.SchulteGrid);
            BindGameButton(colorGridButton, UIWindowType.ColorGrid);
            BindGameButton(dinoRunButton, UIWindowType.DinoRun);
            BindGameButton(dodgeBallButton, UIWindowType.DodgeBall);

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
            }

            ShowReactionPage();
        }

        public override void Open()
        {
            base.Open();
            ShowReactionPage();
        }

        public void ShowReactionPage()
        {
            SetActivePage(reactionPage, focusPage, movementPage);
            UpdateTabVisual(reactionTabImage, focusTabImage, movementTabImage);
        }

        public void ShowFocusPage()
        {
            SetActivePage(focusPage, reactionPage, movementPage);
            UpdateTabVisual(focusTabImage, reactionTabImage, movementTabImage);
        }

        public void ShowMovementPage()
        {
            SetActivePage(movementPage, reactionPage, focusPage);
            UpdateTabVisual(movementTabImage, reactionTabImage, focusTabImage);
        }

        private void BindTabButton(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);
        }

        private void BindGameButton(Button button, UIWindowType detailWindow)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => uiManager.OpenWindow(detailWindow));
        }

        private void SetActivePage(GameObject activePage, GameObject inactivePageA, GameObject inactivePageB)
        {
            if (activePage != null)
            {
                activePage.SetActive(true);
            }

            if (inactivePageA != null)
            {
                inactivePageA.SetActive(false);
            }

            if (inactivePageB != null)
            {
                inactivePageB.SetActive(false);
            }
        }

        private void UpdateTabVisual(Image activeTab, Image inactiveTabA, Image inactiveTabB)
        {
            SetTabColor(activeTab, selectedTabColor);
            SetTabColor(inactiveTabA, normalTabColor);
            SetTabColor(inactiveTabB, normalTabColor);
        }

        private static void SetTabColor(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
        }
    }
}
