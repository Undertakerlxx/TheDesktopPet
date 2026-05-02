using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class SkinWindowController : MonoBehaviour
    {
        private const float CardWidth = 96f;
        private const float CardHeight = 56f;
        private const float PreviewSize = 26f;
        private const float GridSpacing = 8f;

        private readonly List<Button> skinButtons = new();
        private readonly List<Image> cardImages = new();
        private readonly List<Text> labelTexts = new();

        private readonly Color normalCardColor = new(0.20f, 0.15f, 0.11f, 0.95f);
        private readonly Color selectedCardColor = new(0.52f, 0.30f, 0.18f, 0.98f);
        private readonly Color normalLabelColor = new(0.96f, 0.92f, 0.84f, 1f);
        private readonly Color selectedLabelColor = new(1f, 0.96f, 0.76f, 1f);

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
            gridRect.anchorMin = template.anchorMin;
            gridRect.anchorMax = template.anchorMax;
            gridRect.pivot = template.pivot;
            gridRect.anchoredPosition = template.anchoredPosition;
            gridRect.sizeDelta = template.sizeDelta;

            GridLayoutGroup gridLayout = gridObject.GetComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(CardWidth, CardHeight);
            gridLayout.spacing = new Vector2(GridSpacing, GridSpacing);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;

            return gridRect;
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

            skinButtons.Add(button);
            cardImages.Add(cardImage);
            labelTexts.Add(labelText);
        }

        private void CreatePreviewImage(Transform parent, int index)
        {
            GameObject previewObject = new("Preview", typeof(RectTransform), typeof(Image));
            previewObject.transform.SetParent(parent, false);

            RectTransform previewRect = previewObject.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 1f);
            previewRect.anchorMax = new Vector2(0.5f, 1f);
            previewRect.pivot = new Vector2(0.5f, 1f);
            previewRect.anchoredPosition = new Vector2(0f, -10f);
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
            labelRect.anchoredPosition = new Vector2(0f, 5f);
            labelRect.sizeDelta = new Vector2(-10f, 16f);

            Text labelText = labelObject.GetComponent<Text>();
            Text referenceText = featureWindowController != null ? featureWindowController.titleText : null;
            if (referenceText != null)
            {
                labelText.font = referenceText.font;
                labelText.fontSize = 12;
                labelText.fontStyle = FontStyle.Bold;
            }
            else
            {
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                labelText.fontSize = 12;
                labelText.fontStyle = FontStyle.Bold;
            }

            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = normalLabelColor;
            labelText.text = label;
            labelText.raycastTarget = false;
            return labelText;
        }

        private void OnSkinSelected(int index)
        {
            if (skinManager == null)
            {
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
                if (cardImages[index] != null)
                {
                    cardImages[index].color = isSelected ? selectedCardColor : normalCardColor;
                }

                if (labelTexts[index] != null)
                {
                    labelTexts[index].color = isSelected ? selectedLabelColor : normalLabelColor;
                }
            }
        }
    }
}
