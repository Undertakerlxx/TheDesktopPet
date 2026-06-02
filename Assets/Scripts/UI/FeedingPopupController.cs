using System.Collections.Generic;
using DesktopPet.Achievements;
using DesktopPet.Feeding;
using DesktopPet.Kitchen;
using DesktopPet.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    /// <summary>
    /// Displays the standalone feeding popup where dishes are filtered by category and consumed from storage.
    /// </summary>
    public class FeedingPopupController : MonoBehaviour
    {
        private const int RowsPerPage = 6;
        private const int PreferredHappinessBonus = 5;
        private const int PreferredIntimacyBonus = 1;

        private static readonly DishCategory[] Categories =
        {
            DishCategory.Staple,
            DishCategory.Soup,
            DishCategory.Dessert,
            DishCategory.Drink,
            DishCategory.Snack,
            DishCategory.VegetableDish
        };

        private readonly List<Button> categoryButtons = new();
        private readonly List<Text> categoryButtonLabels = new();
        private readonly List<FeedingDishRowView> rowViews = new();

        private DesktopPetProgressService progressService;
        private FeedingService feedingService;
        private ThePetStatsManager petStatsManager;
        private UIManager ownerManager;
        private Text requestText;
        private Text pageText;
        private Button previousPageButton;
        private Button nextPageButton;
        private DishCategory selectedCategory;
        private int currentPage;

        /// <summary>
        /// Shows a feeding popup under the specified parent.
        /// </summary>
        /// <param name="parent">The UI transform that owns the popup.</param>
        /// <returns>The created popup controller.</returns>
        public static FeedingPopupController Show(Transform parent)
        {
            return Show(parent, null);
        }

        /// <summary>
        /// Shows a feeding popup under the specified parent.
        /// </summary>
        /// <param name="parent">The UI transform that owns the popup.</param>
        /// <param name="ownerManager">The UI manager to notify when the popup closes.</param>
        /// <returns>The created popup controller.</returns>
        public static FeedingPopupController Show(Transform parent, UIManager ownerManager)
        {
            Transform oldPopup = parent.Find("FeedingPopup");
            if (oldPopup != null)
            {
                Destroy(oldPopup.gameObject);
            }

            GameObject root = new("FeedingPopup", typeof(RectTransform), typeof(Image), typeof(FeedingPopupController));
            root.transform.SetParent(parent, false);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(470f, 360f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.92f, 0.9f, 0.8f, 0.98f);

            FeedingPopupController popup = root.GetComponent<FeedingPopupController>();
            popup.ownerManager = ownerManager;
            popup.Initialize();
            return popup;
        }

        private void Initialize()
        {
            progressService = new DesktopPetProgressService();
            feedingService = new FeedingService(progressService);
            petStatsManager = FindFirstObjectByType<ThePetStatsManager>();
            selectedCategory = feedingService.EnsureRequest().requestedCategory;

            CreateText(transform, "Title", new Vector2(18f, -14f), new Vector2(120f, 30f), 20, TextAnchor.MiddleLeft).text = "喂食";

            Button closeButton = CreateButton(transform, "关闭", new Vector2(406f, -14f), new Vector2(46f, 28f), 13);
            closeButton.onClick.AddListener(Close);

            requestText = CreateText(transform, "RequestText", new Vector2(22f, -52f), new Vector2(360f, 24f), 14, TextAnchor.MiddleLeft);
            CreateCategoryButtons();
            CreatePageControls();
            CreateRows();
            Refresh();
        }

        private void Close()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            ownerManager?.NotifyFeedingPopupClosed(this);
        }

        private void CreateCategoryButtons()
        {
            Vector2 start = new(22f, -86f);
            Vector2 size = new(82f, 26f);
            for (int i = 0; i < Categories.Length; i++)
            {
                int row = i / 3;
                int column = i % 3;
                DishCategory category = Categories[i];
                Button button = CreateButton(transform, FeedingService.GetCategoryDisplayName(category), start + new Vector2(column * 90f, -row * 31f), size, 12);
                button.onClick.AddListener(() =>
                {
                    selectedCategory = category;
                    currentPage = 0;
                    Refresh();
                });

                categoryButtons.Add(button);
                categoryButtonLabels.Add(button.GetComponentInChildren<Text>());
            }
        }

        private void CreatePageControls()
        {
            previousPageButton = CreateButton(transform, "上一页", new Vector2(292f, -86f), new Vector2(58f, 26f), 12);
            previousPageButton.onClick.AddListener(() => ChangePage(currentPage - 1));

            pageText = CreateText(transform, "PageText", new Vector2(354f, -86f), new Vector2(34f, 26f), 12, TextAnchor.MiddleCenter);

            nextPageButton = CreateButton(transform, "下一页", new Vector2(388f, -86f), new Vector2(58f, 26f), 12);
            nextPageButton.onClick.AddListener(() => ChangePage(currentPage + 1));
        }

        private void CreateRows()
        {
            Vector2 start = new(22f, -156f);
            for (int i = 0; i < RowsPerPage; i++)
            {
                FeedingDishRowView row = FeedingDishRowView.Create(transform, start + new Vector2(0f, -i * 30f), new Vector2(416f, 26f), this);
                rowViews.Add(row);
            }
        }

        private void ChangePage(int nextPage)
        {
            currentPage = Mathf.Clamp(nextPage, 0, GetMaxPage(GetVisibleRecipes().Count));
            Refresh();
        }

        private void Refresh()
        {
            progressService.Reload();
            DishCategory requestedCategory = feedingService.EnsureRequest().requestedCategory;
            requestText.text = $"当前想吃：{FeedingService.GetCategoryDisplayName(requestedCategory)}";

            RefreshCategoryButtons(requestedCategory);

            List<RecipeDefinition> recipes = GetVisibleRecipes();
            int maxPage = GetMaxPage(recipes.Count);
            currentPage = Mathf.Clamp(currentPage, 0, maxPage);
            int firstIndex = currentPage * RowsPerPage;

            for (int i = 0; i < rowViews.Count; i++)
            {
                int recipeIndex = firstIndex + i;
                rowViews[i].Refresh(recipeIndex < recipes.Count ? recipes[recipeIndex] : null);
            }

            pageText.text = $"{currentPage + 1}/{maxPage + 1}";
            previousPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < maxPage;
        }

        private void RefreshCategoryButtons(DishCategory requestedCategory)
        {
            for (int i = 0; i < categoryButtons.Count; i++)
            {
                DishCategory category = Categories[i];
                bool selected = category == selectedCategory;
                bool requested = category == requestedCategory;

                Image image = categoryButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    image.color = selected
                        ? new Color(0.98f, 0.82f, 0.46f, 1f)
                        : requested
                            ? new Color(0.84f, 1f, 0.82f, 0.95f)
                            : new Color(1f, 0.95f, 0.84f, 0.96f);
                }

                if (categoryButtonLabels[i] != null)
                {
                    categoryButtonLabels[i].text = requested
                        ? $"{FeedingService.GetCategoryDisplayName(category)}*"
                        : FeedingService.GetCategoryDisplayName(category);
                }
            }
        }

        private List<RecipeDefinition> GetVisibleRecipes()
        {
            List<RecipeDefinition> recipes = new();
            foreach (RecipeDefinition recipe in KitchenDatabase.Recipes)
            {
                bool unlocked = KitchenDatabase.IsRecipeUnlocked(recipe.id, progressService.Data.FarmLevel);
                if (unlocked && recipe.category == selectedCategory)
                {
                    recipes.Add(recipe);
                }
            }

            return recipes;
        }

        private bool TryFeed(RecipeDefinition recipe, FeedingDishRowView sourceRow)
        {
            bool success = feedingService.TryFeed(recipe.id, out FeedingResult result);
            if (success)
            {
                AchievementEventRecorder.Record(AchievementEventType.Feed);
                if (result.matchedPreference)
                {
                    AchievementEventRecorder.Record(AchievementEventType.PreferredFeed);
                }

                string statFeedback = ApplyRecipeStats(recipe, result.matchedPreference);
                sourceRow.ShowFeedback($"{result.message} {statFeedback}");
            }

            Refresh();
            return success;
        }

        private string ApplyRecipeStats(RecipeDefinition recipe, bool matchedPreference)
        {
            if (petStatsManager == null)
            {
                petStatsManager = FindFirstObjectByType<ThePetStatsManager>();
            }

            int happinessRestore = recipe.happinessRestore + (matchedPreference ? PreferredHappinessBonus : 0);
            int intimacyRestore = matchedPreference ? PreferredIntimacyBonus : 0;

            ThePetStats stats = petStatsManager != null ? petStatsManager.current_stats : null;
            if (petStatsManager == null || stats == null)
            {
                return BuildStatFeedback(recipe.satietyRestore, happinessRestore, intimacyRestore, intimacyRestore > 0);
            }

            float satietyBefore = stats.satiety;
            float happinessBefore = stats.happiness;
            float intimacyBefore = stats.intimacy;

            if (!petStatsManager.ApplyFeedingEffect(recipe.satietyRestore, happinessRestore, intimacyRestore))
            {
                return BuildStatFeedback(recipe.satietyRestore, happinessRestore, intimacyRestore, intimacyRestore > 0);
            }

            return BuildStatFeedback(
                Mathf.Max(0f, stats.satiety - satietyBefore),
                Mathf.Max(0f, stats.happiness - happinessBefore),
                Mathf.Max(0f, stats.intimacy - intimacyBefore),
                intimacyRestore > 0);
        }

        private static string BuildStatFeedback(float satietyDelta, float happinessDelta, float intimacyDelta, bool includeIntimacy)
        {
            string feedback = "\u9971\u98df+" + FormatStatDelta(satietyDelta) + " \u5f00\u5fc3+" + FormatStatDelta(happinessDelta);
            if (includeIntimacy)
            {
                feedback += " \u4eb2\u5bc6+" + FormatStatDelta(intimacyDelta);
            }

            return feedback;
        }

        private static string FormatStatDelta(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value))
                ? Mathf.RoundToInt(value).ToString()
                : value.ToString("0.#");
        }

        private int GetDishAmount(RecipeId recipeId)
        {
            return progressService.GetDishAmount(recipeId);
        }

        private static int GetMaxPage(int count)
        {
            return Mathf.Max(0, Mathf.CeilToInt(count / (float)RowsPerPage) - 1);
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

        private class FeedingDishRowView
        {
            private readonly FeedingPopupController controller;
            private readonly Transform root;
            private readonly Text label;
            private readonly Button feedButton;
            private RecipeDefinition recipe;

            private FeedingDishRowView(FeedingPopupController controller, Transform root, Text label, Button feedButton)
            {
                this.controller = controller;
                this.root = root;
                this.label = label;
                this.feedButton = feedButton;
                this.feedButton.onClick.AddListener(OnFeedClicked);
            }

            public static FeedingDishRowView Create(Transform parent, Vector2 position, Vector2 size, FeedingPopupController controller)
            {
                GameObject root = new("FeedingDishRow", typeof(RectTransform), typeof(Image));
                root.transform.SetParent(parent, false);

                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;

                Image background = root.GetComponent<Image>();
                background.color = new Color(1f, 0.98f, 0.9f, 0.9f);

                Text label = CreateText(root.transform, "Label", new Vector2(8f, -2f), new Vector2(size.x - 80f, size.y), 12, TextAnchor.MiddleLeft);
                Button feedButton = CreateButton(root.transform, "喂食", new Vector2(size.x - 68f, -3f), new Vector2(60f, 22f), 12);
                return new FeedingDishRowView(controller, root.transform, label, feedButton);
            }

            public void Refresh(RecipeDefinition recipe)
            {
                this.recipe = recipe;
                feedButton.gameObject.SetActive(recipe != null);
                if (recipe == null)
                {
                    label.text = string.Empty;
                    return;
                }

                int amount = controller.GetDishAmount(recipe.id);
                label.text = $"{recipe.displayName} x{amount}";
                feedButton.interactable = amount > 0;
            }

            public void ShowFeedback(string message)
            {
                FloatingTextFeedback.Show(root, message, Vector2.zero, new Color(0.18f, 0.55f, 0.22f, 1f));
            }

            private void OnFeedClicked()
            {
                if (recipe != null)
                {
                    controller.TryFeed(recipe, this);
                }
            }
        }
    }
}
