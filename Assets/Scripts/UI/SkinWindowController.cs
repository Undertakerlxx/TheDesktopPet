using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class SkinWindowController : MonoBehaviour
    {
        private const float WindowWidth = 580f;
        private const float WindowHeight = 380f;
        private const float CardWidth = 150f;
        private const float CardHeight = 108f;
        private const float PreviewSize = 70f;
        private const float GridSpacing = 14f;

        private readonly List<Button> skinButtons = new();
        private readonly List<Image> cardImages = new();
        private readonly List<Text> labelTexts = new();
        private readonly List<Text> statusTexts = new();

        private readonly Color normalCardColor = new(0.20f, 0.15f, 0.11f, 0.95f);
        private readonly Color selectedCardColor = new(0.52f, 0.30f, 0.18f, 0.98f);
        private readonly Color lockedCardColor = new(0.13f, 0.13f, 0.13f, 0.88f);
        private readonly Color normalLabelColor = new(0.96f, 0.92f, 0.84f, 1f);
        private readonly Color selectedLabelColor = new(1f, 0.96f, 0.76f, 1f);
        private readonly Color lockedLabelColor = new(0.60f, 0.60f, 0.60f, 1f);
        private readonly Color statusLabelColor = new(0.78f, 0.72f, 0.64f, 1f);

        private FeatureWindowController featureWindowController;
        private PetSkinManager skinManager;
        private RectTransform gridRoot;
        private bool hasBuiltUi;

        private void Awake()
        {
            AutoWireReferences();
            BuildUiIfNeeded();
        }

        private void OnEnable()
        {
            AutoWireReferences();
            BuildUiIfNeeded();
            RefreshSelectionState();
        }

        private void AutoWireReferences()
        {
            if (featureWindowController == null)
            {
                featureWindowController = GetComponent<FeatureWindowController>();
            }

            if (skinManager == null)
            {
                skinManager = FindFirstObjectByType<PetSkinManager>();
            }

            if (featureWindowController != null && featureWindowController.titleText != null)
            {
                featureWindowController.titleText.text = "皮肤选择";
            }

            ConfigureWindowLayout();
        }

        private void ConfigureWindowLayout()
        {
            if (featureWindowController == null)
            {
                return;
            }

            RectTransform windowRect = featureWindowController.GetComponent<RectTransform>();
            if (windowRect != null)
            {
                windowRect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
            }

            if (featureWindowController.titleText != null)
            {
                Text titleText = featureWindowController.titleText;
                titleText.fontSize = 32;
                titleText.alignment = TextAnchor.MiddleLeft;
                titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
                titleText.verticalOverflow = VerticalWrapMode.Truncate;

                RectTransform titleRect = titleText.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.pivot = new Vector2(0f, 1f);
                titleRect.offsetMin = new Vector2(32f, -82f);
                titleRect.offsetMax = new Vector2(-128f, -22f);
            }

            if (featureWindowController.closeButton != null)
            {
                RectTransform closeRect = featureWindowController.closeButton.GetComponent<RectTransform>();
                if (closeRect != null)
                {
                    closeRect.anchorMin = new Vector2(1f, 1f);
                    closeRect.anchorMax = new Vector2(1f, 1f);
                    closeRect.pivot = new Vector2(1f, 1f);
                    closeRect.sizeDelta = new Vector2(68f, 44f);
                    closeRect.anchoredPosition = new Vector2(-28f, -24f);
                }

                Text closeLabel = featureWindowController.closeButton.GetComponentInChildren<Text>();
                if (closeLabel != null)
                {
                    closeLabel.fontSize = 19;
                    closeLabel.alignment = TextAnchor.MiddleCenter;
                }
            }

            if (featureWindowController.bodyText != null)
            {
                RectTransform bodyRect = featureWindowController.bodyText.rectTransform;
                bodyRect.anchorMin = Vector2.zero;
                bodyRect.anchorMax = Vector2.one;
                bodyRect.pivot = new Vector2(0.5f, 0.5f);
                bodyRect.offsetMin = new Vector2(32f, 34f);
                bodyRect.offsetMax = new Vector2(-32f, -100f);

                if (gridRoot != null)
                {
                    ApplyGridRootLayout(gridRoot, bodyRect);
                    ConfigureGridLayout(gridRoot.GetComponent<GridLayoutGroup>());
                }
            }
        }

        private void BuildUiIfNeeded()
        {
            if (hasBuiltUi || featureWindowController == null || skinManager == null)
            {
                return;
            }

            int skinCount = skinManager.GetSkinCount();
            if (skinCount <= 0)
            {
                return;
            }

            RectTransform bodyRect = featureWindowController.bodyText != null
                ? featureWindowController.bodyText.rectTransform
                : null;
            if (bodyRect == null)
            {
                return;
            }

            featureWindowController.bodyText.gameObject.SetActive(false);
            ClearCachedUi();
            gridRoot = GetOrCreateGridRoot(bodyRect);
            ClearGridChildren();

            for (int index = 0; index < skinCount; index++)
            {
                CreateSkinCard(index);
            }

            hasBuiltUi = true;
        }

        private RectTransform GetOrCreateGridRoot(RectTransform template)
        {
            Transform existingGrid = template.parent.Find("SkinGrid");
            GameObject gridObject;
            if (existingGrid != null)
            {
                gridObject = existingGrid.gameObject;
                if (gridObject.GetComponent<GridLayoutGroup>() == null)
                {
                    gridObject.AddComponent<GridLayoutGroup>();
                }
            }
            else
            {
                gridObject = new GameObject("SkinGrid", typeof(RectTransform), typeof(GridLayoutGroup));
                gridObject.transform.SetParent(template.parent, false);
            }

            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            ApplyGridRootLayout(gridRect, template);

            GridLayoutGroup gridLayout = gridObject.GetComponent<GridLayoutGroup>();
            ConfigureGridLayout(gridLayout);

            return gridRect;
        }

        private static void ApplyGridRootLayout(RectTransform gridRect, RectTransform template)
        {
            gridRect.anchorMin = template.anchorMin;
            gridRect.anchorMax = template.anchorMax;
            gridRect.pivot = template.pivot;
            gridRect.anchoredPosition = template.anchoredPosition;
            gridRect.sizeDelta = template.sizeDelta;
            gridRect.offsetMin = template.offsetMin;
            gridRect.offsetMax = template.offsetMax;
        }

        private static void ConfigureGridLayout(GridLayoutGroup gridLayout)
        {
            if (gridLayout == null)
            {
                return;
            }

            gridLayout.cellSize = new Vector2(CardWidth, CardHeight);
            gridLayout.spacing = new Vector2(GridSpacing, GridSpacing);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        }

        private void ClearGridChildren()
        {
            if (gridRoot == null)
            {
                return;
            }

            for (int index = gridRoot.childCount - 1; index >= 0; index--)
            {
                DestroyImmediate(gridRoot.GetChild(index).gameObject);
            }
        }

        private void ClearCachedUi()
        {
            skinButtons.Clear();
            cardImages.Clear();
            labelTexts.Clear();
            statusTexts.Clear();
        }

        private void CreateSkinCard(int index)
        {
            GameObject cardObject = new($"SkinCard_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            cardObject.transform.SetParent(gridRoot, false);

            Image cardImage = cardObject.GetComponent<Image>();
            cardImage.color = normalCardColor;

            Button button = cardObject.GetComponent<Button>();
            button.targetGraphic = cardImage;
            int capturedIndex = index;
            button.onClick.AddListener(() => OnSkinSelected(capturedIndex));

            CreatePreviewImage(cardObject.transform, index);
            Text labelText = CreateLabel(cardObject.transform, skinManager.GetSkinDisplayName(index));
            Text statusText = CreateStatusText(cardObject.transform);

            skinButtons.Add(button);
            cardImages.Add(cardImage);
            labelTexts.Add(labelText);
            statusTexts.Add(statusText);
        }

        private void CreatePreviewImage(Transform parent, int index)
        {
            GameObject previewObject = new("Preview", typeof(RectTransform), typeof(Image));
            previewObject.transform.SetParent(parent, false);

            RectTransform previewRect = previewObject.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 1f);
            previewRect.anchorMax = new Vector2(0.5f, 1f);
            previewRect.pivot = new Vector2(0.5f, 1f);
            previewRect.anchoredPosition = new Vector2(0f, -6f);
            previewRect.sizeDelta = new Vector2(PreviewSize, PreviewSize);

            Image previewImage = previewObject.GetComponent<Image>();
            previewImage.sprite = skinManager.GetSkinPreviewSprite(index);
            previewImage.preserveAspect = true;
        }

        private Text CreateLabel(Transform parent, string label)
        {
            GameObject labelObject = new("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 24f);
            labelRect.sizeDelta = new Vector2(-12f, 24f);

            Text labelText = labelObject.GetComponent<Text>();
            Text referenceText = featureWindowController != null ? featureWindowController.titleText : null;
            if (referenceText != null)
            {
                labelText.font = referenceText.font;
                labelText.fontSize = 16;
                labelText.fontStyle = FontStyle.Bold;
            }
            else
            {
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                labelText.fontSize = 16;
                labelText.fontStyle = FontStyle.Bold;
            }

            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = normalLabelColor;
            labelText.text = label;
            labelText.raycastTarget = false;
            return labelText;
        }

        private Text CreateStatusText(Transform parent)
        {
            GameObject statusObject = new("Status", typeof(RectTransform), typeof(Text));
            statusObject.transform.SetParent(parent, false);

            RectTransform statusRect = statusObject.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(1f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 6f);
            statusRect.sizeDelta = new Vector2(-10f, 24f);

            Text statusText = statusObject.GetComponent<Text>();
            Text referenceText = featureWindowController != null ? featureWindowController.titleText : null;
            statusText.font = referenceText != null
                ? referenceText.font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 12;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = statusLabelColor;
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Truncate;
            statusText.raycastTarget = false;
            return statusText;
        }

        private void OnSkinSelected(int index)
        {
            if (skinManager == null)
            {
                return;
            }

            if (!skinManager.IsSkinUnlocked(index))
            {
                string description = skinManager.GetSkinUnlockDescription(index);
                string progress = skinManager.GetSkinUnlockProgressText(index);
                FloatingTextFeedback.Show(transform, string.IsNullOrEmpty(description) ? progress : description, Vector2.zero, new Color(0.90f, 0.68f, 0.30f, 1f));
                return;
            }

            if (skinManager.ApplySkin(index))
            {
                RefreshSelectionState();
            }
        }

        private void RefreshSelectionState()
        {
            if (!hasBuiltUi || skinManager == null)
            {
                return;
            }

            int selectedIndex = skinManager.GetSelectedSkinIndex();
            for (int index = 0; index < cardImages.Count; index++)
            {
                bool isSelected = index == selectedIndex;
                bool isUnlocked = skinManager.IsSkinUnlocked(index);
                if (index < skinButtons.Count && skinButtons[index] != null)
                {
                    skinButtons[index].interactable = true;
                }

                if (cardImages[index] != null)
                {
                    cardImages[index].color = !isUnlocked
                        ? lockedCardColor
                        : (isSelected ? selectedCardColor : normalCardColor);
                }

                if (labelTexts[index] != null)
                {
                    labelTexts[index].color = !isUnlocked
                        ? lockedLabelColor
                        : (isSelected ? selectedLabelColor : normalLabelColor);
                }

                if (index < statusTexts.Count && statusTexts[index] != null)
                {
                    statusTexts[index].text = isUnlocked
                        ? (isSelected ? "使用中" : "已解锁")
                        : skinManager.GetSkinUnlockProgressText(index);
                    statusTexts[index].color = isUnlocked ? statusLabelColor : lockedLabelColor;
                }
            }
        }
    }
}
