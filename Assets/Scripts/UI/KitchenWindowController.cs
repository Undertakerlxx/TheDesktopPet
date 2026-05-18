using System;
using System.Collections.Generic;
using DesktopPet.Catalog;
using DesktopPet.Inventory;
using DesktopPet.Kitchen;
using DesktopPet.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    /// <summary>
    /// Controls the temporary kitchen UI, including recipe selection, cooking jobs, feeding entry, storage entry, and catalog entry.
    /// </summary>
    public class KitchenWindowController : UIWindowController
    {
        private const int RecipesPerPage = 6;
        private const int JobsPerPage = 3;

        public Text titleText;
        public RectTransform contentRoot;
        public Button closeButton;

        private readonly List<Button> recipeButtons = new();
        private readonly List<Text> recipeButtonLabels = new();
        private readonly List<KitchenJobView> jobViews = new();

        private DesktopPetProgressService progressService;
        private KitchenService kitchenService;
        private Text statusText;
        private Text detailText;
        private Text pageText;
        private Button previousPageButton;
        private Button nextPageButton;
        private Button cookButton;
        private RecipeId selectedRecipeId = RecipeId.TomatoSoup;
        private int currentRecipePage;

        /// <inheritdoc />
        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            progressService = new DesktopPetProgressService();
            kitchenService = new KitchenService(progressService);

            AutoWireReferences();
            BuildTemporaryUi();
            Refresh();
        }

        /// <inheritdoc />
        public override void Open()
        {
            base.Open();
            progressService.Reload();
            Refresh();
        }

        private void Update()
        {
            if (windowLayer != null && windowLayer.IsVisible)
            {
                RefreshJobs();
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
                titleText.text = "厨房";
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
            }

            FitKitchenWindow();
            FitHeader();
        }

        private void FitKitchenWindow()
        {
            RectTransform windowRect = GetComponent<RectTransform>();
            if (windowRect != null)
            {
                windowRect.sizeDelta = new Vector2(560f, 460f);
            }

            if (contentRoot == null)
            {
                return;
            }

            contentRoot.anchorMin = Vector2.zero;
            contentRoot.anchorMax = Vector2.one;
            contentRoot.offsetMin = new Vector2(30f, 18f);
            contentRoot.offsetMax = new Vector2(-18f, -72f);
        }

        private void FitHeader()
        {
            if (titleText != null)
            {
                RectTransform titleRect = titleText.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(0f, 1f);
                titleRect.pivot = new Vector2(0f, 1f);
                titleRect.anchoredPosition = new Vector2(28f, -22f);
                titleRect.sizeDelta = new Vector2(180f, 42f);
            }

            if (closeButton != null)
            {
                RectTransform closeRect = closeButton.GetComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-22f, -18f);
                closeRect.sizeDelta = new Vector2(48f, 34f);
            }
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

            recipeButtons.Clear();
            recipeButtonLabels.Clear();
            jobViews.Clear();
            currentRecipePage = Mathf.Clamp(currentRecipePage, 0, GetMaxRecipePage());

            statusText = CreateText(contentRoot, "KitchenStatus", new Vector2(10f, -8f), new Vector2(300f, 26f), 15, TextAnchor.MiddleLeft);
            CreateEntryButtons();
            CreateText(contentRoot, "RecipeTitle", new Vector2(10f, -42f), new Vector2(70f, 24f), 14, TextAnchor.MiddleLeft).text = "菜谱";
            CreatePageControls();
            CreateRecipeButtons();

            CreateText(contentRoot, "DetailTitle", new Vector2(240f, -42f), new Vector2(110f, 24f), 14, TextAnchor.MiddleLeft).text = "料理详情";
            detailText = CreateText(contentRoot, "RecipeDetail", new Vector2(250f, -72f), new Vector2(250f, 150f), 13, TextAnchor.UpperLeft);
            cookButton = CreateButton(contentRoot, "开始烹饪", new Vector2(250f, -230f), new Vector2(100f, 30f), 13);
            cookButton.onClick.AddListener(TryCookSelectedRecipe);

            CreateJobViews();
        }

        private void CreatePageControls()
        {
            previousPageButton = CreateButton(contentRoot, "上一页", new Vector2(74f, -42f), new Vector2(58f, 24f), 12);
            previousPageButton.onClick.AddListener(() => ChangeRecipePage(currentRecipePage - 1));

            pageText = CreateText(contentRoot, "RecipePage", new Vector2(136f, -42f), new Vector2(42f, 24f), 12, TextAnchor.MiddleCenter);

            nextPageButton = CreateButton(contentRoot, "下一页", new Vector2(182f, -42f), new Vector2(58f, 24f), 12);
            nextPageButton.onClick.AddListener(() => ChangeRecipePage(currentRecipePage + 1));
        }

        private void CreateEntryButtons()
        {
            Button feedingButton = CreateButton(contentRoot, "喂食", new Vector2(330f, -8f), new Vector2(52f, 28f), 13);
            feedingButton.onClick.AddListener(() => FeedingPopupController.Show(transform));

            Button storageButton = CreateButton(contentRoot, "仓库", new Vector2(390f, -8f), new Vector2(52f, 28f), 13);
            storageButton.onClick.AddListener(() => StoragePopupController.Show(transform, StorageTab.Dishes));

            Button catalogButton = CreateButton(contentRoot, "图鉴", new Vector2(450f, -8f), new Vector2(52f, 28f), 13);
            catalogButton.onClick.AddListener(() => CatalogPopupController.Show(transform, CatalogEntryType.Recipe));
        }

        private void CreateRecipeButtons()
        {
            Vector2 start = new(10f, -72f);
            Vector2 size = new(200f, 28f);
            int firstRecipeIndex = currentRecipePage * RecipesPerPage;
            int visibleCount = Mathf.Min(RecipesPerPage, KitchenDatabase.Recipes.Count - firstRecipeIndex);

            for (int i = 0; i < visibleCount; i++)
            {
                int recipeIndex = firstRecipeIndex + i;
                RecipeDefinition recipe = KitchenDatabase.Recipes[recipeIndex];
                Button button = CreateButton(contentRoot, recipe.displayName, start + new Vector2(0f, -i * 32f), size, 12);
                RecipeId recipeId = recipe.id;
                button.onClick.AddListener(() =>
                {
                    selectedRecipeId = recipeId;
                    Refresh();
                });

                recipeButtons.Add(button);
                recipeButtonLabels.Add(button.GetComponentInChildren<Text>());
            }
        }

        private void CreateJobViews()
        {
            Vector2 start = new(10f, -266f);
            Vector2 size = new(470f, 30f);

            for (int i = 0; i < JobsPerPage; i++)
            {
                KitchenJobView view = KitchenJobView.Create(contentRoot, start + new Vector2(0f, -i * 34f), size, this);
                jobViews.Add(view);
            }
        }

        private void ChangeRecipePage(int nextPage)
        {
            int clampedPage = Mathf.Clamp(nextPage, 0, GetMaxRecipePage());
            if (clampedPage == currentRecipePage)
            {
                return;
            }

            currentRecipePage = clampedPage;
            BuildTemporaryUi();
            Refresh();
        }

        private void Refresh()
        {
            if (kitchenService == null || statusText == null)
            {
                return;
            }

            progressService.Reload();
            statusText.text = $"厨房经验 {kitchenService.Progress.kitchenExperience}  农场等级 Lv{kitchenService.FarmLevel}";
            RefreshRecipeButtons();
            RefreshSelectedRecipe();
            RefreshPageControls();
            RefreshJobs();
        }

        private void RefreshRecipeButtons()
        {
            int firstRecipeIndex = currentRecipePage * RecipesPerPage;
            for (int i = 0; i < recipeButtons.Count; i++)
            {
                RecipeDefinition recipe = KitchenDatabase.Recipes[firstRecipeIndex + i];
                bool unlocked = kitchenService.IsUnlocked(recipe.id);
                bool selected = recipe.id == selectedRecipeId;
                bool canCook = kitchenService.CanStartCooking(recipe.id);

                Button button = recipeButtons[i];
                button.interactable = unlocked;

                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = selected
                        ? new Color(0.98f, 0.82f, 0.46f, 1f)
                        : canCook
                            ? new Color(0.84f, 1f, 0.82f, 0.95f)
                            : unlocked
                                ? new Color(1f, 0.95f, 0.84f, 0.96f)
                                : new Color(0.78f, 0.78f, 0.78f, 0.72f);
                }

                if (recipeButtonLabels[i] != null)
                {
                    recipeButtonLabels[i].text = unlocked ? recipe.displayName : $"Lv{recipe.unlockFarmLevel} 解锁";
                }
            }
        }

        private void RefreshSelectedRecipe()
        {
            RecipeDefinition recipe = KitchenDatabase.GetRecipe(selectedRecipeId);
            bool unlocked = kitchenService.IsUnlocked(recipe.id);
            bool canCook = kitchenService.CanStartCooking(recipe.id);

            detailText.text =
                $"{recipe.displayName}  [{GetTierText(recipe.tier)}]\n" +
                $"分类：{DesktopPet.Feeding.FeedingService.GetCategoryDisplayName(recipe.category)}\n" +
                $"解锁：农场 Lv{recipe.unlockFarmLevel}  时间：{FormatDuration(kitchenService.GetCookDuration(recipe))}\n" +
                $"效果：饱食 +{recipe.satietyRestore}  开心 +{recipe.happinessRestore}\n" +
                $"经验：厨房 +{recipe.kitchenExperience}\n" +
                $"材料：{BuildIngredientText(recipe)}\n" +
                $"{recipe.description}";

            cookButton.interactable = unlocked && canCook;
            Text cookLabel = cookButton.GetComponentInChildren<Text>();
            if (cookLabel != null)
            {
                cookLabel.text = !unlocked ? "未解锁" : canCook ? "开始烹饪" : "材料不足";
            }
        }

        private void RefreshPageControls()
        {
            int maxPage = GetMaxRecipePage();
            pageText.text = $"{currentRecipePage + 1}/{maxPage + 1}";
            previousPageButton.interactable = currentRecipePage > 0;
            nextPageButton.interactable = currentRecipePage < maxPage;
        }

        private void RefreshJobs()
        {
            for (int i = 0; i < jobViews.Count; i++)
            {
                CookingJobState job = i < kitchenService.Progress.cookingJobs.Count
                    ? kitchenService.Progress.cookingJobs[i]
                    : null;
                jobViews[i].Refresh(job);
            }
        }

        private void TryCookSelectedRecipe()
        {
            kitchenService.TryStartCooking(selectedRecipeId);
            Refresh();
        }

        private bool TryCompleteJob(CookingJobState job)
        {
            if (job == null)
            {
                return false;
            }

            RecipeDefinition recipe = KitchenDatabase.GetRecipe(job.recipeId);
            bool result = kitchenService.TryComplete(job);
            if (result)
            {
                KitchenJobView view = FindJobView(job);
                if (view != null)
                {
                    view.ShowFeedback($"{recipe.displayName} x1");
                }
            }

            Refresh();
            return result;
        }

        private KitchenJobView FindJobView(CookingJobState job)
        {
            foreach (KitchenJobView view in jobViews)
            {
                if (view.HasJob(job))
                {
                    return view;
                }
            }

            return null;
        }

        private string GetJobLabel(CookingJobState job)
        {
            if (job == null)
            {
                return "空闲灶台";
            }

            RecipeDefinition recipe = KitchenDatabase.GetRecipe(job.recipeId);
            if (job.completed)
            {
                return $"{recipe.displayName} 可完成";
            }

            TimeSpan remaining = kitchenService.GetRemainingTime(job);
            return remaining == TimeSpan.Zero
                ? $"{recipe.displayName} 可完成"
                : $"{recipe.displayName} 剩余 {FormatRemaining(remaining)}";
        }

        private bool CanCompleteJob(CookingJobState job)
        {
            return kitchenService.CanComplete(job);
        }

        private string BuildIngredientText(RecipeDefinition recipe)
        {
            List<string> parts = new();
            foreach (IngredientRequirement ingredient in recipe.ingredients)
            {
                int owned = progressService.GetItemAmount(ingredient.itemId);
                string name = InventoryDatabase.GetDisplayName(ingredient.itemId);
                parts.Add($"{name} {owned}/{ingredient.amount}");
            }

            return string.Join("，", parts);
        }

        private static int GetMaxRecipePage()
        {
            return Mathf.Max(0, Mathf.CeilToInt(KitchenDatabase.Recipes.Count / (float)RecipesPerPage) - 1);
        }

        private static string GetTierText(RecipeTier tier)
        {
            return tier switch
            {
                RecipeTier.Basic => "基础",
                RecipeTier.Intermediate => "进阶",
                RecipeTier.Advanced => "高级",
                _ => tier.ToString()
            };
        }

        private static string FormatRemaining(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1d)
            {
                return $"{(int)timeSpan.TotalHours}时{timeSpan.Minutes:D2}分";
            }

            return $"{timeSpan.Minutes:D2}分{timeSpan.Seconds:D2}秒";
        }

        private static string FormatDuration(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes >= 1d)
            {
                return $"{(int)timeSpan.TotalMinutes}分";
            }

            return $"{timeSpan.Seconds}秒";
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

        private class KitchenJobView
        {
            private readonly KitchenWindowController controller;
            private readonly Transform root;
            private readonly Text label;
            private readonly Button actionButton;
            private CookingJobState job;

            private KitchenJobView(KitchenWindowController controller, Transform root, Text label, Button actionButton)
            {
                this.controller = controller;
                this.root = root;
                this.label = label;
                this.actionButton = actionButton;
                this.actionButton.onClick.AddListener(OnActionClicked);
            }

            public static KitchenJobView Create(Transform parent, Vector2 position, Vector2 size, KitchenWindowController controller)
            {
                GameObject root = new("KitchenJob", typeof(RectTransform), typeof(Image));
                root.transform.SetParent(parent, false);

                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;

                Image background = root.GetComponent<Image>();
                background.color = new Color(1f, 0.98f, 0.9f, 0.9f);

                Text label = CreateText(root.transform, "JobLabel", new Vector2(8f, -3f), new Vector2(size.x - 92f, size.y - 4f), 12, TextAnchor.MiddleLeft);
                Button actionButton = CreateButton(root.transform, "完成", new Vector2(size.x - 76f, -3f), new Vector2(68f, 24f), 12);
                return new KitchenJobView(controller, root.transform, label, actionButton);
            }

            public void Refresh(CookingJobState job)
            {
                this.job = job;
                label.text = controller.GetJobLabel(job);
                actionButton.interactable = controller.CanCompleteJob(job);
            }

            public bool HasJob(CookingJobState targetJob)
            {
                return job == targetJob;
            }

            public void ShowFeedback(string message)
            {
                FloatingTextFeedback.Show(root, message, new Vector2(0f, 4f), new Color(0.18f, 0.55f, 0.22f, 1f));
            }

            private void OnActionClicked()
            {
                controller.TryCompleteJob(job);
            }
        }
    }
}
