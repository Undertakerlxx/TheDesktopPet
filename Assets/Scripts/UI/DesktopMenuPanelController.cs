using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class DesktopMenuPanelController : UIPanelController
    {
        private const string RuntimeFeedingButtonName = "RuntimeFeedingButton";

        public Button skinButton;
        public Button timerButton;
        public Button miniGameButton;
        public Button farmButton;
        public Button kitchenButton;
        public Button feedingButton;
        public Button achievementButton;

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
                feedingButton = CreateRuntimeFeedingButton();
            }

            if (feedingButton == null)
            {
                return;
            }

            feedingButton.onClick.RemoveAllListeners();
            feedingButton.onClick.AddListener(() => uiManager.OpenFeedingPopup());
        }

        private Button CreateRuntimeFeedingButton()
        {
            Transform oldButton = transform.Find(RuntimeFeedingButtonName);
            if (oldButton != null)
            {
                return oldButton.GetComponent<Button>();
            }

            Button template = kitchenButton != null ? kitchenButton : farmButton;
            if (template == null)
            {
                return null;
            }

            GameObject obj = Instantiate(template.gameObject, transform);
            obj.name = RuntimeFeedingButtonName;

            RectTransform rect = obj.GetComponent<RectTransform>();
            RectTransform templateRect = template.GetComponent<RectTransform>();
            if (rect != null && templateRect != null)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(68f, 28f);
                rect.anchoredPosition = new Vector2(-8f, -8f);
            }

            obj.transform.SetAsLastSibling();

            Text label = obj.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "喂食";
            }

            return obj.GetComponent<Button>();
        }
    }
}
