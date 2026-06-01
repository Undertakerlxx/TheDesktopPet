using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class GameSettingsPopupController : MonoBehaviour
    {
        private UIManager ownerManager;
        private global::WindowController windowController;
        private Text topmostValueText;
        private Text frameRateValueText;
        private Text petScaleValueText;
        private Text statsValueText;
        private Text escapeQuitValueText;

        private bool topmostEnabled;
        private int frameRateIndex;
        private int petScaleIndex;
        private bool statsDisplayEnabled;
        private bool escapeQuitEnabled;

        public static GameSettingsPopupController Show(Transform parent, UIManager ownerManager)
        {
            Transform oldPopup = parent.Find("GameSettingsPopup");
            if (oldPopup != null)
            {
                Destroy(oldPopup.gameObject);
            }

            GameObject root = new("GameSettingsPopup", typeof(RectTransform), typeof(Image), typeof(GameSettingsPopupController));
            root.transform.SetParent(parent, false);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(500f, 450f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.90f, 0.93f, 0.97f, 0.98f);

            GameSettingsPopupController popup = root.GetComponent<GameSettingsPopupController>();
            popup.ownerManager = ownerManager;
            popup.Initialize();
            return popup;
        }

        private void Initialize()
        {
            windowController = FindFirstObjectByType<global::WindowController>();
            topmostEnabled = GameSettingsStore.IsTopmostEnabled();
            frameRateIndex = GameSettingsStore.GetFrameRateIndex();
            petScaleIndex = GameSettingsStore.GetPetScaleIndex();
            statsDisplayEnabled = GameSettingsStore.IsStatsDisplayEnabled();
            escapeQuitEnabled = GameSettingsStore.IsEscapeQuitEnabled();
            CreateText(transform, "Title", new Vector2(18f, -14f), new Vector2(220f, 30f), 20, TextAnchor.MiddleLeft).text = "\u5168\u5c40\u8bbe\u7f6e";

            Button closeButton = CreateButton(transform, "\u5173\u95ed", new Vector2(436f, -14f), new Vector2(46f, 28f), 13);
            closeButton.onClick.AddListener(Close);

            CreateSettingRow("\u7a97\u53e3\u7f6e\u9876", 70f, out topmostValueText, ToggleTopmost);
            CreateSettingRow("\u5e27\u7387\u6a21\u5f0f", 118f, out frameRateValueText, CycleFrameRate);
            CreateSettingRow("\u684c\u5ba0\u5927\u5c0f", 166f, out petScaleValueText, CyclePetScale);
            CreateSettingRow("\u5c5e\u6027\u680f\u663e\u793a", 214f, out statsValueText, ToggleStatsDisplay);
            CreateSettingRow("Esc \u9000\u51fa", 262f, out escapeQuitValueText, ToggleEscapeQuit);

            Button quitButton = CreateButton(transform, "\u9000\u51fa\u6e38\u620f", new Vector2(268f, -320f), new Vector2(170f, 30f), 13);
            quitButton.onClick.AddListener(QuitGame);

            Button defaultButton = CreateButton(transform, "\u6062\u590d\u9ed8\u8ba4", new Vector2(42f, -320f), new Vector2(120f, 32f), 13);
            defaultButton.onClick.AddListener(ResetToDefault);

            Button switchAccountButton = CreateButton(transform, "\u5207\u6362\u8d26\u53f7", new Vector2(180f, -320f), new Vector2(120f, 32f), 13);
            switchAccountButton.onClick.AddListener(SwitchAccount);

            Text tipText = CreateText(transform, "TipText", new Vector2(22f, -370f), new Vector2(450f, 22f), 12, TextAnchor.MiddleLeft);
            tipText.text = "\u70b9\u51fb\u53f3\u4fa7\u6309\u94ae\u5207\u6362\u8bbe\u7f6e\uff0c\u4e5f\u53ef\u4ee5\u5728\u8fd9\u91cc\u5207\u6362\u8d26\u53f7\u6216\u9000\u51fa\u6e38\u620f\u3002";

            RefreshTexts();
            ApplyCurrentSettings();
        }

        private void CreateSettingRow(string label, float top, out Text valueText, UnityEngine.Events.UnityAction action)
        {
            CreateText(transform, $"{label}Label", new Vector2(24f, -top), new Vector2(170f, 30f), 15, TextAnchor.MiddleLeft).text = label;
            Button valueButton = CreateButton(transform, $"{label}Value", new Vector2(268f, -top), new Vector2(170f, 30f), 13);
            valueButton.onClick.AddListener(action);
            valueText = valueButton.GetComponentInChildren<Text>();
        }

        private void ToggleTopmost()
        {
            topmostEnabled = !topmostEnabled;
            GameSettingsStore.SetTopmostEnabled(topmostEnabled);
            ApplyCurrentSettings();
            RefreshTexts();
        }

        private void CycleFrameRate()
        {
            frameRateIndex = (frameRateIndex + 1) % GameSettingsStore.FrameRateLabels.Length;
            GameSettingsStore.SetFrameRateIndex(frameRateIndex);
            ApplyCurrentSettings();
            RefreshTexts();
        }

        private void CyclePetScale()
        {
            petScaleIndex = (petScaleIndex + 1) % GameSettingsStore.PetScaleLabels.Length;
            GameSettingsStore.SetPetScaleIndex(petScaleIndex);
            ApplyCurrentSettings();
            RefreshTexts();
        }

        private void ToggleStatsDisplay()
        {
            statsDisplayEnabled = !statsDisplayEnabled;
            GameSettingsStore.SetStatsDisplayEnabled(statsDisplayEnabled);
            ApplyCurrentSettings();
            RefreshTexts();
        }

        private void ToggleEscapeQuit()
        {
            escapeQuitEnabled = !escapeQuitEnabled;
            GameSettingsStore.SetEscapeQuitEnabled(escapeQuitEnabled);
            ApplyCurrentSettings();
            RefreshTexts();
        }

        private void ResetToDefault()
        {
            GameSettingsStore.ResetToDefault();
            topmostEnabled = GameSettingsStore.IsTopmostEnabled();
            frameRateIndex = GameSettingsStore.GetFrameRateIndex();
            petScaleIndex = GameSettingsStore.GetPetScaleIndex();
            statsDisplayEnabled = GameSettingsStore.IsStatsDisplayEnabled();
            escapeQuitEnabled = GameSettingsStore.IsEscapeQuitEnabled();
            ApplyCurrentSettings();
            RefreshTexts();
        }

        private void SwitchAccount()
        {
            DesktopPet.Accounts.LoginBootstrap bootstrap = FindFirstObjectByType<DesktopPet.Accounts.LoginBootstrap>();
            Close();
            bootstrap?.SwitchAccount();
        }

        private void ApplyCurrentSettings()
        {
            Application.targetFrameRate = GameSettingsStore.GetTargetFrameRate();
            windowController?.SetTopmost(topmostEnabled);
            windowController?.SetEscapeToQuit(escapeQuitEnabled);
            ownerManager?.SetPetScaleMultiplier(GameSettingsStore.GetPetScaleMultiplier());
            ownerManager?.SetStatsDisplayEnabled(statsDisplayEnabled);
        }

        private void RefreshTexts()
        {
            if (topmostValueText != null) topmostValueText.text = topmostEnabled ? "\u5f00\u542f" : "\u5173\u95ed";
            if (frameRateValueText != null) frameRateValueText.text = GameSettingsStore.FrameRateLabels[frameRateIndex];
            if (petScaleValueText != null) petScaleValueText.text = GameSettingsStore.PetScaleLabels[petScaleIndex];
            if (statsValueText != null) statsValueText.text = statsDisplayEnabled ? "\u663e\u793a" : "\u9690\u85cf";
            if (escapeQuitValueText != null) escapeQuitValueText.text = escapeQuitEnabled ? "\u5f00\u542f" : "\u5173\u95ed";
        }

        private void Close()
        {
            Destroy(gameObject);
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            ownerManager?.NotifyGameSettingsPopupClosed(this);
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = fontSize;
            text.color = new Color(0.18f, 0.14f, 0.1f, 1f);
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            GameObject obj = new(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = obj.GetComponent<Image>();
            image.color = new Color(1f, 0.98f, 0.92f, 0.96f);

            Button button = obj.GetComponent<Button>();
            Text text = CreateText(obj.transform, "Label", Vector2.zero, size, fontSize, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            text.text = label;
            return button;
        }
    }
}
