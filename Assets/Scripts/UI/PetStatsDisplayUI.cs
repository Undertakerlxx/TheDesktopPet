using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class PetStatsDisplayUI : MonoBehaviour
    {
        private const string FeedingButtonName = "StatsFeedingButton";

        public ThePet pet;
        public ThePetStatsManager statsManager;
        public RectTransform panel;
        public Text statsText;
        public Canvas targetCanvas;
        public Vector3 worldOffset = new(0f, 1.15f, 0f);
        public Vector2 screenOffset = new(0f, 12f);

        private RectTransform canvasRectTransform;
        private UIManager uiManager;
        private Button feedingButton;
        private Text feedingButtonLabel;

        private void Awake()
        {
            AutoWireReferences();
            EnsureFeedingButton();
        }

        private void LateUpdate()
        {
            AutoWireReferences();
            EnsureFeedingButton();
            UpdatePosition();
            UpdateStatsText();
        }

        private void AutoWireReferences()
        {
            if (pet == null)
            {
                pet = FindFirstObjectByType<ThePet>();
            }

            if (statsManager == null && pet != null)
            {
                statsManager = pet.GetComponent<ThePetStatsManager>();
            }

            if (panel == null)
            {
                panel = GetComponent<RectTransform>();
            }

            if (statsText == null)
            {
                statsText = GetComponentInChildren<Text>(true);
            }

            if (targetCanvas == null)
            {
                targetCanvas = GetComponentInParent<Canvas>();
            }

            if (uiManager == null)
            {
                uiManager = GetComponentInParent<UIManager>();
                if (uiManager == null)
                {
                    uiManager = FindFirstObjectByType<UIManager>();
                }
            }

            if (canvasRectTransform == null && targetCanvas != null)
            {
                canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
            }
        }

        private void UpdatePosition()
        {
            if (pet == null || pet.cam == null || panel == null)
            {
                return;
            }

            Vector3 worldPosition = pet.transform.position + worldOffset;
            Vector2 screenPosition = pet.cam.WorldToScreenPoint(worldPosition);
            screenPosition += screenOffset;

            if (targetCanvas == null || targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                panel.position = screenPosition;
                return;
            }

            if (canvasRectTransform == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPosition,
                targetCanvas.worldCamera,
                out Vector2 localPoint);
            panel.anchoredPosition = localPoint;
        }

        private void UpdateStatsText()
        {
            if (statsText == null || statsManager == null)
            {
                return;
            }

            ThePetStats stats = statsManager.current_stats;
            if (stats == null)
            {
                if (statsManager.stats != null && statsManager.stats.Length > 0)
                {
                    statsManager.Change(0);
                    stats = statsManager.current_stats;
                }

                if (stats == null)
                {
                    statsText.text = "属性未配置";
                    return;
                }
            }

            statsText.text =
                $"亲密度: {stats.intimacy:0}\n" +
                $"开心值: {stats.happiness:0}\n" +
                $"活力值: {stats.energy:0}/{stats.energy_max:0}\n" +
                $"专注值: {stats.focus:0}\n" +
                $"饱食度: {stats.satiety:0}";
        }

        private void EnsureFeedingButton()
        {
            if (panel == null)
            {
                return;
            }

            if (feedingButton == null)
            {
                Transform existingButton = panel.Find(FeedingButtonName);
                feedingButton = existingButton != null ? existingButton.GetComponent<Button>() : null;
            }

            if (feedingButton == null)
            {
                feedingButton = CreateFeedingButton();
            }

            ConfigureFeedingButton();
        }

        private Button CreateFeedingButton()
        {
            GameObject obj = new(FeedingButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(panel, false);

            Text label = CreateFeedingButtonLabel(obj.transform);
            feedingButtonLabel = label;
            return obj.GetComponent<Button>();
        }

        private Text CreateFeedingButtonLabel(Transform parent)
        {
            GameObject labelObject = new("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);

            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.raycastTarget = false;
            return label;
        }

        private void ConfigureFeedingButton()
        {
            if (feedingButton == null)
            {
                return;
            }

            RectTransform rect = feedingButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-9f, 8f);
                rect.sizeDelta = new Vector2(42f, 22f);
            }

            Image image = feedingButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.98f, 0.82f, 0.46f, 0.96f);
            }

            ColorBlock colors = feedingButton.colors;
            colors.normalColor = new Color(0.98f, 0.82f, 0.46f, 0.96f);
            colors.highlightedColor = new Color(1f, 0.90f, 0.58f, 1f);
            colors.pressedColor = new Color(0.86f, 0.64f, 0.30f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.72f, 0.67f, 0.58f, 0.7f);
            colors.colorMultiplier = 1f;
            feedingButton.colors = colors;

            feedingButton.onClick.RemoveAllListeners();
            feedingButton.onClick.AddListener(OpenFeedingPopup);

            if (feedingButtonLabel == null)
            {
                feedingButtonLabel = feedingButton.GetComponentInChildren<Text>(true);
            }

            if (feedingButtonLabel != null)
            {
                Text referenceText = statsText;
                if (referenceText != null)
                {
                    feedingButtonLabel.font = referenceText.font;
                }
                else
                {
                    feedingButtonLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }

                feedingButtonLabel.text = "喂食";
                feedingButtonLabel.fontSize = 12;
                feedingButtonLabel.fontStyle = FontStyle.Bold;
                feedingButtonLabel.alignment = TextAnchor.MiddleCenter;
                feedingButtonLabel.color = new Color(0.24f, 0.16f, 0.08f, 1f);
                feedingButtonLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                feedingButtonLabel.verticalOverflow = VerticalWrapMode.Truncate;
            }

            feedingButton.transform.SetAsLastSibling();
        }

        private void OpenFeedingPopup()
        {
            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
            }

            uiManager?.OpenFeedingPopup();
        }
    }
}
