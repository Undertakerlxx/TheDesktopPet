using System;
using System.Collections.Generic;
using DesktopPet.Catalog;
using DesktopPet.Farm;
using DesktopPet.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    /// <summary>
    /// Controls the temporary farm UI, including seed selection, paged plots, storage entry, and catalog entry.
    /// </summary>
    public class FarmWindowController : UIWindowController
    {
        private const int PlotsPerPage = 4;

        public Text titleText;
        public RectTransform contentRoot;
        public Button closeButton;

        private readonly List<Button> cropButtons = new();
        private readonly List<Text> cropButtonLabels = new();
        private readonly List<FarmPlotView> plotViews = new();

        private DesktopPetProgressService progressService;
        private FarmService farmService;
        private Text statusText;
        private Text inventoryText;
        private Text pageText;
        private Button previousPageButton;
        private Button nextPageButton;
        private Button storageButton;
        private Button catalogButton;
        private CropId selectedCropId = CropId.Tomato;
        private int currentPlotPage;
        private int renderedPlotCount;

        /// <inheritdoc />
        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            progressService = new DesktopPetProgressService();
            farmService = new FarmService(progressService);

            AutoWireReferences();
            BuildTemporaryUi();
            Refresh();
        }

        /// <inheritdoc />
        public override void Open()
        {
            base.Open();
            Refresh();
        }

        private void Update()
        {
            if (windowLayer != null && windowLayer.IsVisible)
            {
                RefreshPlotViews();
            }
        }

        private void AutoWireReferences()
        {
            if (titleText == null)
            {
                Transform title = transform.Find("Title");
                titleText = title != null ? title.GetComponent<Text>() : null;
            }

            if (contentRoot == null)
            {
                Transform body = transform.Find("Body");
                contentRoot = body != null ? body.GetComponent<RectTransform>() : null;
            }

            if (closeButton == null)
            {
                Transform close = transform.Find("CloseButton");
                closeButton = close != null ? close.GetComponent<Button>() : null;
            }

            if (titleText != null)
            {
                titleText.text = "农场";
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
            }

            FitFarmWindow();
        }

        private void FitFarmWindow()
        {
            RectTransform windowRect = GetComponent<RectTransform>();
            if (windowRect != null)
            {
                windowRect.sizeDelta = new Vector2(500f, 420f);
            }

            if (contentRoot == null)
            {
                return;
            }

            contentRoot.anchorMin = Vector2.zero;
            contentRoot.anchorMax = Vector2.one;
            contentRoot.offsetMin = new Vector2(18f, 18f);
            contentRoot.offsetMax = new Vector2(-18f, -72f);
        }

        private void BuildTemporaryUi()
        {
            if (contentRoot == null)
            {
                return;
            }

            Text oldBodyText = contentRoot.GetComponent<Text>();
            if (oldBodyText != null)
            {
                oldBodyText.enabled = false;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }

            cropButtons.Clear();
            cropButtonLabels.Clear();
            plotViews.Clear();

            renderedPlotCount = FarmDatabase.GetPlotCountForLevel(FarmDatabase.MaxLevel);
            currentPlotPage = Mathf.Clamp(currentPlotPage, 0, GetMaxPageIndex(renderedPlotCount));

            statusText = CreateText(contentRoot, "StatusText", new Vector2(0f, -12f), new Vector2(380f, 44f), 16, TextAnchor.UpperLeft);
            inventoryText = CreateText(contentRoot, "InventoryText", new Vector2(0f, -58f), new Vector2(380f, 24f), 13, TextAnchor.UpperLeft);
            CreateEntryButtons();

            CreateText(contentRoot, "SeedTitle", new Vector2(0f, -90f), new Vector2(80f, 24f), 14, TextAnchor.MiddleLeft).text = "选择种子";
            CreateCropButtons();

            CreateText(contentRoot, "PlotTitle", new Vector2(0f, -158f), new Vector2(80f, 24f), 14, TextAnchor.MiddleLeft).text = "田地";
            CreatePageControls();
            CreatePlotViews();
        }

        private void CreateCropButtons()
        {
            const int columns = 5;
            Vector2 start = new(76f, -88f);
            Vector2 size = new(58f, 28f);
            Vector2 gap = new(6f, 6f);

            for (int i = 0; i < FarmDatabase.Crops.Count; i++)
            {
                CropDefinition crop = FarmDatabase.Crops[i];
                int row = i / columns;
                int column = i % columns;
                Vector2 position = start + new Vector2(column * (size.x + gap.x), -row * (size.y + gap.y));

                Button button = CreateButton(contentRoot, crop.displayName, position, size, 12);
                CropId cropId = crop.id;
                button.onClick.AddListener(() =>
                {
                    selectedCropId = cropId;
                    Refresh();
                });

                cropButtons.Add(button);
                cropButtonLabels.Add(button.GetComponentInChildren<Text>());
            }
        }

        private void CreatePageControls()
        {
            previousPageButton = CreateButton(contentRoot, "上一页", new Vector2(278f, -156f), new Vector2(58f, 26f), 12);
            previousPageButton.onClick.AddListener(() => ChangePlotPage(currentPlotPage - 1));

            pageText = CreateText(contentRoot, "PageText", new Vector2(342f, -156f), new Vector2(44f, 26f), 12, TextAnchor.MiddleCenter);

            nextPageButton = CreateButton(contentRoot, "下一页", new Vector2(392f, -156f), new Vector2(58f, 26f), 12);
            nextPageButton.onClick.AddListener(() => ChangePlotPage(currentPlotPage + 1));
        }

        private void CreateEntryButtons()
        {
            storageButton = CreateButton(contentRoot, "仓库", new Vector2(350f, -18f), new Vector2(52f, 28f), 13);
            storageButton.onClick.AddListener(() => StoragePopupController.Show(transform, StorageTab.Crops));

            catalogButton = CreateButton(contentRoot, "图鉴", new Vector2(410f, -18f), new Vector2(52f, 28f), 13);
            catalogButton.onClick.AddListener(() => CatalogPopupController.Show(transform, CatalogEntryType.Crop));
        }

        private void CreatePlotViews()
        {
            const int columns = 2;
            Vector2 start = new(0f, -190f);
            Vector2 size = new(218f, 50f);
            Vector2 gap = new(12f, 10f);
            int firstPlotIndex = currentPlotPage * PlotsPerPage;
            int visiblePlotCount = Mathf.Min(PlotsPerPage, renderedPlotCount - firstPlotIndex);

            for (int i = 0; i < visiblePlotCount; i++)
            {
                int row = i / columns;
                int column = i % columns;
                int plotIndex = firstPlotIndex + i;
                Vector2 position = start + new Vector2(column * (size.x + gap.x), -row * (size.y + gap.y));
                FarmPlotView view = FarmPlotView.Create(contentRoot, plotIndex, position, size, this);
                plotViews.Add(view);
            }
        }

        private void ChangePlotPage(int nextPage)
        {
            int maxPage = GetMaxPageIndex(renderedPlotCount);
            int clampedPage = Mathf.Clamp(nextPage, 0, maxPage);
            if (clampedPage == currentPlotPage)
            {
                return;
            }

            currentPlotPage = clampedPage;
            BuildTemporaryUi();
            Refresh();
        }

        private void Refresh()
        {
            if (farmService == null || statusText == null)
            {
                return;
            }

            if (!FarmDatabase.IsCropUnlocked(selectedCropId, farmService.FarmLevel))
            {
                selectedCropId = CropId.Tomato;
            }

            int maxPlotCount = FarmDatabase.GetPlotCountForLevel(FarmDatabase.MaxLevel);
            if (maxPlotCount != renderedPlotCount || currentPlotPage > GetMaxPageIndex(maxPlotCount))
            {
                renderedPlotCount = maxPlotCount;
                BuildTemporaryUi();
                Refresh();
                return;
            }

            CropDefinition selectedCrop = FarmDatabase.GetCrop(selectedCropId);
            string experienceText = BuildExperienceText();
            statusText.text =
                $"等级 Lv{farmService.FarmLevel}  经验 {experienceText}\n" +
                $"当前种子：{selectedCrop.displayName}";

            if (inventoryText != null)
            {
                inventoryText.text = BuildInventoryText();
            }

            RefreshCropButtons();
            RefreshPageControls();
            RefreshPlotViews();
        }

        private string BuildInventoryText()
        {
            CropDefinition selectedCrop = FarmDatabase.GetCrop(selectedCropId);
            int amount = progressService.GetItemAmount(FarmDatabase.GetHarvestItem(selectedCrop.id));
            return $"库存：{selectedCrop.displayName} x{amount}";
        }

        private string BuildExperienceText()
        {
            int requiredExperience = FarmDatabase.GetExperienceToNextLevel(farmService.FarmLevel);
            if (requiredExperience <= 0)
            {
                return "满级";
            }

            int currentLevelExperience = FarmDatabase.GetExperienceInCurrentLevel(farmService.Progress.farmExperience);
            return $"{currentLevelExperience}/{requiredExperience}";
        }

        private void RefreshCropButtons()
        {
            for (int i = 0; i < cropButtons.Count; i++)
            {
                CropDefinition crop = FarmDatabase.Crops[i];
                bool unlocked = FarmDatabase.IsCropUnlocked(crop.id, farmService.FarmLevel);
                bool selected = crop.id == selectedCropId;

                Button button = cropButtons[i];
                button.interactable = unlocked;

                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = selected
                        ? new Color(0.98f, 0.82f, 0.46f, 1f)
                        : unlocked
                            ? new Color(1f, 0.95f, 0.84f, 0.96f)
                            : new Color(0.78f, 0.78f, 0.78f, 0.72f);
                }

                if (cropButtonLabels[i] != null)
                {
                    cropButtonLabels[i].text = unlocked ? crop.displayName : "???";
                }
            }
        }

        private void RefreshPageControls()
        {
            int maxPage = GetMaxPageIndex(renderedPlotCount);
            if (pageText != null)
            {
                pageText.text = $"{currentPlotPage + 1}/{maxPage + 1}";
            }

            if (previousPageButton != null)
            {
                previousPageButton.interactable = currentPlotPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = currentPlotPage < maxPage;
            }
        }

        private void RefreshPlotViews()
        {
            foreach (FarmPlotView view in plotViews)
            {
                view.Refresh();
            }
        }

        private bool TryPlant(int plotIndex)
        {
            bool result = farmService.TryPlant(plotIndex, selectedCropId);
            Refresh();
            return result;
        }

        private bool TryHarvest(int plotIndex)
        {
            bool result = farmService.TryHarvest(plotIndex, out CropId harvestedCropId, out int amount);
            Refresh();
            if (result)
            {
                FarmPlotView view = FindPlotView(plotIndex);
                if (view != null)
                {
                    CropDefinition crop = FarmDatabase.GetCrop(harvestedCropId);
                    view.ShowFeedback($"{crop.displayName} x{amount}");
                }
            }

            return result;
        }

        private FarmPlotView FindPlotView(int plotIndex)
        {
            foreach (FarmPlotView view in plotViews)
            {
                if (view.PlotIndex == plotIndex)
                {
                    return view;
                }
            }

            return null;
        }

        private string GetPlotLabel(int plotIndex)
        {
            if (!IsPlotUnlocked(plotIndex))
            {
                return $"田地 {plotIndex + 1}\n未解锁";
            }

            FarmPlotState plot = farmService.GetPlot(plotIndex);
            if (plot == null || !plot.isPlanted)
            {
                return $"田地 {plotIndex + 1}\n空闲";
            }

            CropDefinition crop = FarmDatabase.GetCrop(plot.cropId);
            TimeSpan remaining = farmService.GetRemainingTime(plot);
            string state = remaining == TimeSpan.Zero ? "可收获" : FormatRemaining(remaining);
            return $"田地 {plotIndex + 1}：{crop.displayName}\n{state}";
        }

        private bool IsPlotUnlocked(int plotIndex)
        {
            return plotIndex < FarmDatabase.GetPlotCountForLevel(farmService.FarmLevel);
        }

        private bool IsPlotPlanted(int plotIndex)
        {
            FarmPlotState plot = farmService.GetPlot(plotIndex);
            return plot != null && plot.isPlanted;
        }

        private bool IsPlotMature(int plotIndex)
        {
            FarmPlotState plot = farmService.GetPlot(plotIndex);
            return plot != null && plot.isPlanted && farmService.IsMature(plot);
        }

        private static int GetMaxPageIndex(int plotCount)
        {
            return Mathf.Max(0, Mathf.CeilToInt(plotCount / (float)PlotsPerPage) - 1);
        }

        private static string FormatRemaining(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1d)
            {
                return $"{(int)timeSpan.TotalHours}时{timeSpan.Minutes:D2}分";
            }

            return $"{timeSpan.Minutes:D2}分{timeSpan.Seconds:D2}秒";
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
            image.color = new Color(1f, 0.95f, 0.84f, 0.96f);

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

        private class FarmPlotView
        {
            private readonly FarmWindowController controller;
            private readonly int plotIndex;
            private readonly Transform root;
            private readonly Text label;
            private readonly Button actionButton;
            private readonly Text actionText;
            private readonly Image background;

            public int PlotIndex => plotIndex;

            private FarmPlotView(FarmWindowController controller, int plotIndex, Transform root, Text label, Button actionButton, Image background)
            {
                this.controller = controller;
                this.plotIndex = plotIndex;
                this.root = root;
                this.label = label;
                this.actionButton = actionButton;
                this.actionText = actionButton.GetComponentInChildren<Text>();
                this.background = background;
                this.actionButton.onClick.AddListener(OnActionClicked);
            }

            public static FarmPlotView Create(Transform parent, int plotIndex, Vector2 position, Vector2 size, FarmWindowController controller)
            {
                GameObject root = new($"Plot{plotIndex + 1}", typeof(RectTransform), typeof(Image));
                root.transform.SetParent(parent, false);

                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;

                Image background = root.GetComponent<Image>();
                background.color = new Color(1f, 0.98f, 0.9f, 0.9f);

                Text label = CreateText(root.transform, "PlotLabel", new Vector2(8f, -5f), new Vector2(size.x - 76f, size.y - 8f), 12, TextAnchor.UpperLeft);
                Button actionButton = CreateButton(root.transform, "播种", new Vector2(size.x - 62f, -9f), new Vector2(54f, 30f), 12);

                FarmPlotView view = new(controller, plotIndex, root.transform, label, actionButton, background);
                view.Refresh();
                return view;
            }

            public void Refresh()
            {
                bool planted = controller.IsPlotPlanted(plotIndex);
                bool mature = controller.IsPlotMature(plotIndex);
                bool unlocked = controller.IsPlotUnlocked(plotIndex);

                label.text = controller.GetPlotLabel(plotIndex);
                actionButton.interactable = unlocked && (!planted || mature);
                if (!unlocked)
                {
                    actionText.text = "锁定";
                    background.color = new Color(0.78f, 0.78f, 0.78f, 0.55f);
                    return;
                }

                actionText.text = planted ? "收获" : "播种";
                background.color = planted
                    ? mature
                        ? new Color(0.82f, 1f, 0.72f, 0.95f)
                        : new Color(0.84f, 0.92f, 1f, 0.9f)
                    : new Color(1f, 0.98f, 0.9f, 0.9f);
            }

            private void OnActionClicked()
            {
                if (!controller.IsPlotPlanted(plotIndex))
                {
                    controller.TryPlant(plotIndex);
                    return;
                }

                controller.TryHarvest(plotIndex);
            }

            public void ShowFeedback(string message)
            {
                FloatingTextFeedback.Show(root, message, new Vector2(0f, 8f), new Color(0.18f, 0.55f, 0.22f, 1f));
            }
        }
    }
}
