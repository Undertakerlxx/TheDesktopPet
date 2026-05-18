using System.Collections.Generic;
using DesktopPet.Inventory;
using DesktopPet.Kitchen;
using DesktopPet.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    /// <summary>
    /// Identifies the active storage tab.
    /// </summary>
    public enum StorageTab
    {
        Crops,
        Dishes
    }

    /// <summary>
    /// Displays the shared storage popup used by farm and kitchen entry points.
    /// </summary>
    public class StoragePopupController : MonoBehaviour
    {
        private const int ItemsPerPage = 8;

        private readonly List<Text> rowTexts = new();
        private DesktopPetProgressService progressService;
        private Text titleText;
        private Text statusText;
        private Text pageText;
        private Button cropTabButton;
        private Button dishTabButton;
        private Button previousPageButton;
        private Button nextPageButton;
        private StorageTab currentTab;
        private int currentPage;

        /// <summary>
        /// Shows a storage popup under the specified parent.
        /// </summary>
        /// <param name="parent">The UI transform that owns the popup.</param>
        /// <param name="defaultTab">The tab selected when the popup opens.</param>
        /// <returns>The created popup controller.</returns>
        public static StoragePopupController Show(Transform parent, StorageTab defaultTab)
        {
            Transform oldPopup = parent.Find("StoragePopup");
            if (oldPopup != null)
            {
                Destroy(oldPopup.gameObject);
            }

            GameObject root = new("StoragePopup", typeof(RectTransform), typeof(Image), typeof(StoragePopupController));
            root.transform.SetParent(parent, false);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(380f, 320f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.93f, 0.9f, 0.78f, 0.98f);

            StoragePopupController popup = root.GetComponent<StoragePopupController>();
            popup.Initialize(defaultTab);
            return popup;
        }

        private void Initialize(StorageTab defaultTab)
        {
            progressService = new DesktopPetProgressService();
            currentTab = defaultTab;

            titleText = CreateText(transform, "Title", new Vector2(18f, -14f), new Vector2(120f, 30f), 20, TextAnchor.MiddleLeft);
            Button closeButton = CreateButton(transform, "关闭", new Vector2(318f, -14f), new Vector2(46f, 28f), 13);
            closeButton.onClick.AddListener(() => Destroy(gameObject));

            cropTabButton = CreateButton(transform, "农作物", new Vector2(18f, -54f), new Vector2(78f, 28f), 13);
            cropTabButton.onClick.AddListener(() => SwitchTab(StorageTab.Crops));

            dishTabButton = CreateButton(transform, "菜品", new Vector2(104f, -54f), new Vector2(78f, 28f), 13);
            dishTabButton.onClick.AddListener(() => SwitchTab(StorageTab.Dishes));

            previousPageButton = CreateButton(transform, "上一页", new Vector2(198f, -54f), new Vector2(58f, 28f), 12);
            previousPageButton.onClick.AddListener(() => ChangePage(currentPage - 1));

            pageText = CreateText(transform, "Page", new Vector2(260f, -54f), new Vector2(38f, 28f), 12, TextAnchor.MiddleCenter);

            nextPageButton = CreateButton(transform, "下一页", new Vector2(302f, -54f), new Vector2(58f, 28f), 12);
            nextPageButton.onClick.AddListener(() => ChangePage(currentPage + 1));

            statusText = CreateText(transform, "StorageStatus", new Vector2(22f, -86f), new Vector2(330f, 22f), 13, TextAnchor.MiddleLeft);

            for (int i = 0; i < ItemsPerPage; i++)
            {
                Text row = CreateText(transform, "StorageRow", new Vector2(24f, -114f - i * 23f), new Vector2(312f, 22f), 13, TextAnchor.MiddleLeft);
                rowTexts.Add(row);
            }

            Refresh();
        }

        private void SwitchTab(StorageTab nextTab)
        {
            currentTab = nextTab;
            currentPage = 0;
            Refresh();
        }

        private void ChangePage(int nextPage)
        {
            currentPage = Mathf.Clamp(nextPage, 0, GetMaxPage());
            Refresh();
        }

        private void Refresh()
        {
            progressService.Reload();
            titleText.text = "仓库";
            RefreshTabButton(cropTabButton, currentTab == StorageTab.Crops);
            RefreshTabButton(dishTabButton, currentTab == StorageTab.Dishes);

            List<string> rows = currentTab == StorageTab.Crops ? BuildCropRows() : BuildDishRows();
            int maxPage = GetMaxPage(rows.Count);
            currentPage = Mathf.Clamp(currentPage, 0, maxPage);
            int firstIndex = currentPage * ItemsPerPage;

            statusText.text = currentTab == StorageTab.Crops ? "农作物用于厨房料理。" : "菜品可在喂食界面中使用。";

            for (int i = 0; i < rowTexts.Count; i++)
            {
                int rowIndex = firstIndex + i;
                rowTexts[i].text = rowIndex < rows.Count ? rows[rowIndex] : string.Empty;
            }

            pageText.text = $"{currentPage + 1}/{maxPage + 1}";
            previousPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < maxPage;
        }

        private List<string> BuildCropRows()
        {
            List<string> rows = new();
            foreach (InventoryItemDefinition item in InventoryDatabase.Items)
            {
                int amount = progressService.GetItemAmount(item.id);
                rows.Add($"{item.displayName} x{amount}");
            }

            return rows;
        }

        private List<string> BuildDishRows()
        {
            List<string> rows = new();
            foreach (RecipeDefinition recipe in KitchenDatabase.Recipes)
            {
                int amount = progressService.GetDishAmount(recipe.id);
                rows.Add($"{recipe.displayName} x{amount}");
            }

            return rows;
        }

        private int GetMaxPage()
        {
            int count = currentTab == StorageTab.Crops ? InventoryDatabase.Items.Count : KitchenDatabase.Recipes.Count;
            return GetMaxPage(count);
        }

        private static int GetMaxPage(int count)
        {
            return Mathf.Max(0, Mathf.CeilToInt(count / (float)ItemsPerPage) - 1);
        }

        private static void RefreshTabButton(Button button, bool selected)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = selected ? new Color(0.98f, 0.82f, 0.46f, 1f) : new Color(1f, 0.95f, 0.84f, 0.96f);
            }
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
    }
}
