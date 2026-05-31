using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class DesktopMenuPanelController : UIPanelController
    {
        private const string RuntimeFeedingButtonName = "RuntimeFeedingButton";
        private const string RuntimeSettingsButtonName = "RuntimeSettingsButton";
        private const float RowSplit = 0.5f;

        public Button skinButton;
        public Button timerButton;
        public Button miniGameButton;
        public Button farmButton;
        public Button kitchenButton;
        public Button feedingButton;
        public Button achievementButton;
        public Button settingsButton;
        public Button exitButton;

        public override void Initialize(UIManager manager, UIPanelLayer layer)
        {
            base.Initialize(manager, layer);
            BindButton(skinButton, UIWindowType.Skin);
            BindButton(timerButton, UIWindowType.Timer);
            BindButton(miniGameButton, UIWindowType.MiniGame);
            BindButton(farmButton, UIWindowType.Farm);
            BindButton(kitchenButton, UIWindowType.Kitchen);
            BindFeedingButton();
            BindButton(achievementButton, UIWindowType.Achievement);
            BindSettingsButton();
            RemoveExitButton();
            LayoutButtons();
        }

        private void BindButton(Button button, UIWindowType windowType)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => uiManager.OpenWindow(windowType));
        }

        private void BindFeedingButton()
        {
            if (feedingButton == null)
            {
                feedingButton = CreateRuntimeUtilityButton(RuntimeFeedingButtonName, "\u5582\u98df");
            }

            if (feedingButton == null)
            {
                return;
            }

            feedingButton.onClick.RemoveAllListeners();
            feedingButton.onClick.AddListener(() => uiManager.OpenFeedingPopup());
        }

        private void BindSettingsButton()
        {
            if (settingsButton == null)
            {
                settingsButton = CreateRuntimeUtilityButton(RuntimeSettingsButtonName, "\u8bbe\u7f6e");
            }

            if (settingsButton == null)
            {
                return;
            }

            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => uiManager.OpenGameSettingsPopup());
        }

        private Button CreateRuntimeUtilityButton(string objectName, string labelText)
        {
            Transform oldButton = transform.Find(objectName);
            if (oldButton != null)
            {
                Button existingButton = oldButton.GetComponent<Button>();
                Text existingLabel = oldButton.GetComponentInChildren<Text>();
                if (existingLabel != null)
                {
                    existingLabel.text = labelText;
                }

                return existingButton;
            }

            Button template = skinButton != null ? skinButton : (achievementButton != null ? achievementButton : kitchenButton);
            if (template == null)
            {
                return null;
            }

            GameObject obj = Object.Instantiate(template.gameObject, transform);
            obj.name = objectName;
            obj.transform.SetAsLastSibling();

            Text label = obj.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = labelText;
            }

            return obj.GetComponent<Button>();
        }

        private void LayoutButtons()
        {
            ConfigureButtonSlot(skinButton, 0f, 0.25f, RowSplit, 1f);
            ConfigureButtonSlot(timerButton, 0.25f, 0.5f, RowSplit, 1f);
            ConfigureButtonSlot(miniGameButton, 0.5f, 0.75f, RowSplit, 1f);
            ConfigureButtonSlot(farmButton, 0.75f, 1f, RowSplit, 1f);

            ConfigureButtonSlot(kitchenButton, 0f, 0.25f, 0f, RowSplit);
            ConfigureButtonSlot(settingsButton, 0.25f, 0.5f, 0f, RowSplit);
            ConfigureButtonSlot(feedingButton, 0.5f, 0.75f, 0f, RowSplit);
            ConfigureButtonSlot(achievementButton, 0.75f, 1f, 0f, RowSplit);
        }

        private void RemoveExitButton()
        {
            if (exitButton != null)
            {
                Object.Destroy(exitButton.gameObject);
                exitButton = null;
            }

            Transform runtimeExit = transform.Find("RuntimeExitButton");
            if (runtimeExit != null)
            {
                Object.Destroy(runtimeExit.gameObject);
            }
        }

        private static void ConfigureButtonSlot(Button button, float minX, float maxX, float minY, float maxY)
        {
            if (button == null)
            {
                return;
            }

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect == null)
            {
                return;
            }

            buttonRect.anchorMin = new Vector2(minX, minY);
            buttonRect.anchorMax = new Vector2(maxX, maxY);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = Vector2.zero;

            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                RectTransform labelRect = label.GetComponent<RectTransform>();
                if (labelRect != null)
                {
                    labelRect.anchorMin = Vector2.zero;
                    labelRect.anchorMax = Vector2.one;
                    labelRect.pivot = new Vector2(0.5f, 0.5f);
                    labelRect.anchoredPosition = Vector2.zero;
                    labelRect.sizeDelta = Vector2.zero;
                }

                label.fontSize = 18;
                label.alignment = TextAnchor.MiddleCenter;
                label.resizeTextForBestFit = false;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                label.verticalOverflow = VerticalWrapMode.Truncate;
            }
        }
    }
}