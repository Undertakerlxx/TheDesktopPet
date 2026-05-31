using System.Collections.Generic;
using DesktopPet.MiniGame;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class MiniGameHubWindowController : UIWindowController
    {
        private struct MiniGameButtonBinding
        {
            public Button button;
            public MiniGameKind gameKind;

            public MiniGameButtonBinding(Button button, MiniGameKind gameKind)
            {
                this.button = button;
                this.gameKind = gameKind;
            }
        }

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

        private readonly List<MiniGameButtonBinding> miniGameButtons = new();
        private readonly Dictionary<Button, string> originalButtonLabels = new();
        private ThePetStatsManager cachedStatsManager;

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            miniGameButtons.Clear();
            originalButtonLabels.Clear();

            BindTabButton(reactionTabButton, ShowReactionPage);
            BindTabButton(focusTabButton, ShowFocusPage);
            BindTabButton(movementTabButton, ShowMovementPage);
            BindGameButton(eyeHandSpeedButton, UIWindowType.EyeHandSpeed, MiniGameKind.EyeHandSpeed);
            BindGameButton(geometryAtAGlanceButton, UIWindowType.GeometryAtAGlance, MiniGameKind.GeometryAtAGlance);
            BindGameButton(schulteGridButton, UIWindowType.SchulteGrid, MiniGameKind.SchulteGrid);
            BindGameButton(colorGridButton, UIWindowType.ColorGrid, MiniGameKind.ColorGrid);
            BindGameButton(dinoRunButton, UIWindowType.DinoRun, MiniGameKind.DinoRun);
            BindGameButton(dodgeBallButton, UIWindowType.DodgeBall, MiniGameKind.DodgeBall);

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
            }

            RefreshMiniGameButtons();
            ShowReactionPage();
        }

        public override void Open()
        {
            base.Open();
            RefreshMiniGameButtons();
            ShowReactionPage();
        }

        public void ShowReactionPage()
        {
            RefreshMiniGameButtons();
            SetActivePage(reactionPage, focusPage, movementPage);
            UpdateTabVisual(reactionTabImage, focusTabImage, movementTabImage);
        }

        public void ShowFocusPage()
        {
            RefreshMiniGameButtons();
            SetActivePage(focusPage, reactionPage, movementPage);
            UpdateTabVisual(focusTabImage, reactionTabImage, movementTabImage);
        }

        public void ShowMovementPage()
        {
            RefreshMiniGameButtons();
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

        private void BindGameButton(Button button, UIWindowType detailWindow, MiniGameKind gameKind)
        {
            if (button == null)
            {
                return;
            }

            CacheButtonLabel(button);
            miniGameButtons.Add(new MiniGameButtonBinding(button, gameKind));

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => uiManager.OpenWindow(detailWindow));
        }

        private void RefreshMiniGameButtons()
        {
            cachedStatsManager ??= FindFirstObjectByType<ThePetStatsManager>();

            foreach (MiniGameButtonBinding binding in miniGameButtons)
            {
                if (binding.button == null)
                {
                    continue;
                }

                bool unlocked = cachedStatsManager == null || cachedStatsManager.IsMiniGameUnlocked(binding.gameKind);
                int requiredIntimacy = ThePetStatsManager.GetMiniGameUnlockRequirement(binding.gameKind);
                binding.button.interactable = unlocked;

                if (!originalButtonLabels.TryGetValue(binding.button, out string originalLabel))
                {
                    continue;
                }

                Text label = binding.button.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = unlocked || requiredIntimacy <= 0
                        ? originalLabel
                        : $"{originalLabel}\n亲密{requiredIntimacy}";
                }
            }
        }

        private void CacheButtonLabel(Button button)
        {
            if (button == null || originalButtonLabels.ContainsKey(button))
            {
                return;
            }

            Text label = button.GetComponentInChildren<Text>(true);
            originalButtonLabels[button] = label != null ? label.text : string.Empty;
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
