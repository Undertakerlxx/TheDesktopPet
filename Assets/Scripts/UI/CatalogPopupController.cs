using System.Collections.Generic;
using DesktopPet.Catalog;
using DesktopPet.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    /// <summary>
    /// Displays the catalog popup used by farm and kitchen entry points.
    /// </summary>
    public class CatalogPopupController : MonoBehaviour
    {
        private const int EntriesPerPage = 6;

        private readonly List<Text> rowTexts = new();
        private DesktopPetProgressService progressService;
        private Text titleText;
        private Text pageText;
        private Button cropTabButton;
        private Button recipeTabButton;
        private Button previousPageButton;
        private Button nextPageButton;
        private CatalogEntryType currentType;
        private int currentPage;

        /// <summary>
        /// Shows a catalog popup under the specified parent.
        /// </summary>
        /// <param name="parent">The UI transform that owns the popup.</param>
        /// <param name="defaultType">The catalog entry type selected when the popup opens.</param>
        /// <returns>The created popup controller.</returns>
        public static CatalogPopupController Show(Transform parent, CatalogEntryType defaultType)
        {
            Transform oldPopup = parent.Find("CatalogPopup");
            if (oldPopup != null)
            {
                Destroy(oldPopup.gameObject);
            }

            GameObject root = new("CatalogPopup", typeof(RectTransform), typeof(Image), typeof(CatalogPopupController));
            root.transform.SetParent(parent, false);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(390f, 320f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.88f, 0.93f, 0.9f, 0.98f);

            CatalogPopupController popup = root.GetComponent<CatalogPopupController>();
            popup.Initialize(defaultType);
            return popup;
        }

        private void Initialize(CatalogEntryType defaultType)
        {
            progressService = new DesktopPetProgressService();
            currentType = defaultType;

            titleText = CreateText(transform, "Title", new Vector2(18f, -14f), new Vector2(120f, 30f), 20, TextAnchor.MiddleLeft);
            Button closeButton = CreateButton(transform, "关闭", new Vector2(330f, -14f), new Vector2(46f, 28f), 13);
            closeButton.onClick.AddListener(() => Destroy(gameObject));

            cropTabButton = CreateButton(transform, "作物", new Vector2(18f, -54f), new Vector2(70f, 28f), 13);
            cropTabButton.onClick.AddListener(() => SwitchType(CatalogEntryType.Crop));

            recipeTabButton = CreateButton(transform, "菜品", new Vector2(96f, -54f), new Vector2(70f, 28f), 13);
            recipeTabButton.onClick.AddListener(() => SwitchType(CatalogEntryType.Recipe));

            previousPageButton = CreateButton(transform, "上一页", new Vector2(198f, -54f), new Vector2(58f, 28f), 12);
            previousPageButton.onClick.AddListener(() => ChangePage(currentPage - 1));

            pageText = CreateText(transform, "Page", new Vector2(260f, -54f), new Vector2(38f, 28f), 12, TextAnchor.MiddleCenter);

            nextPageButton = CreateButton(transform, "下一页", new Vector2(302f, -54f), new Vector2(58f, 28f), 12);
            nextPageButton.onClick.AddListener(() => ChangePage(currentPage + 1));

            for (int i = 0; i < EntriesPerPage; i++)
            {
                Text row = CreateText(transform, "CatalogRow", new Vector2(22f, -94f - i * 34f), new Vector2(342f, 32f), 12, TextAnchor.UpperLeft);
                rowTexts.Add(row);
            }

            Refresh();
        }

        private void SwitchType(CatalogEntryType nextType)
        {
            currentType = nextType;
            currentPage = 0;
            Refresh();
        }

        private void ChangePage(int nextPage)
        {
            currentPage = Mathf.Clamp(nextPage, 0, GetMaxPage(GetEntries().Count));
            Refresh();
        }

        private void Refresh()
        {
            titleText.text = "图鉴";
            RefreshTabButton(cropTabButton, currentType == CatalogEntryType.Crop);
            RefreshTabButton(recipeTabButton, currentType == CatalogEntryType.Recipe);

            List<CatalogEntryDefinition> entries = GetEntries();
            int maxPage = GetMaxPage(entries.Count);
            currentPage = Mathf.Clamp(currentPage, 0, maxPage);
            int firstIndex = currentPage * EntriesPerPage;

            for (int i = 0; i < rowTexts.Count; i++)
            {
                int entryIndex = firstIndex + i;
                rowTexts[i].text = entryIndex < entries.Count ? BuildEntryText(entries[entryIndex]) : string.Empty;
            }

            pageText.text = $"{currentPage + 1}/{maxPage + 1}";
            previousPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < maxPage;
        }

        private List<CatalogEntryDefinition> GetEntries()
        {
            List<CatalogEntryDefinition> entries = new();
            foreach (CatalogEntryDefinition entry in CatalogDatabase.Entries)
            {
                if (entry.type == currentType)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private string BuildEntryText(CatalogEntryDefinition entry)
        {
            bool unlocked = progressService.IsCatalogEntryUnlocked(entry.id);
            if (!unlocked)
            {
                return $"???\n{entry.unlockHint}";
            }

            return $"{entry.displayName}\n{entry.description}";
        }

        private static int GetMaxPage(int count)
        {
            return Mathf.Max(0, Mathf.CeilToInt(count / (float)EntriesPerPage) - 1);
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
