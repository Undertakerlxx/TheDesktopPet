using System.Collections.Generic;
using DesktopPet.Achievements;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class AchievementWindowController : UIWindowController
    {
        private const int RowsPerPage = 4;

        public Text titleText;
        public RectTransform contentRoot;
        public Button closeButton;

        private readonly AchievementCategory[] categories =
        {
            AchievementCategory.Growth,
            AchievementCategory.Daily,
            AchievementCategory.Challenge,
            AchievementCategory.Collection,
            AchievementCategory.Hidden
        };

        private readonly List<Button> categoryButtons = new();
        private readonly List<AchievementRowView> rowViews = new();

        private AchievementService achievementService;
        private Text statusText;
        private Text pageText;
        private Button previousPageButton;
        private Button nextPageButton;
        private AchievementCategory selectedCategory = AchievementCategory.Growth;
        private int currentPage;

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            achievementService = new AchievementService();
            AutoWireReferences();
            BuildUi();
            Refresh();
        }

        public override void Open()
        {
            base.Open();
            Refresh();
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
                titleText.text = "成就";
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
            }

            RectTransform windowRect = GetComponent<RectTransform>();
            if (windowRect != null)
            {
                windowRect.sizeDelta = new Vector2(520f, 420f);
            }

            if (contentRoot != null)
            {
                contentRoot.anchorMin = Vector2.zero;
                contentRoot.anchorMax = Vector2.one;
                contentRoot.offsetMin = new Vector2(18f, 18f);
                contentRoot.offsetMax = new Vector2(-18f, -70f);
            }
        }

        private void BuildUi()
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

            categoryButtons.Clear();
            rowViews.Clear();

            statusText = CreateText(contentRoot, "StatusText", new Vector2(0f, -8f), new Vector2(292f, 34f), 14, TextAnchor.UpperLeft);
            CreateCategoryButtons();
            CreateRows();
            CreatePageControls();
        }

        private void CreateCategoryButtons()
        {
            Vector2 start = new(0f, -52f);
            Vector2 size = new(78f, 28f);
            Vector2 gap = new(8f, 0f);

            for (int i = 0; i < categories.Length; i++)
            {
                AchievementCategory category = categories[i];
                Button button = CreateButton(contentRoot, AchievementDatabase.GetCategoryDisplayName(category), start + new Vector2(i * (size.x + gap.x), 0f), size, 13);
                button.onClick.AddListener(() =>
                {
                    selectedCategory = category;
                    currentPage = 0;
                    Refresh();
                });
                categoryButtons.Add(button);
            }
        }

        private void CreateRows()
        {
            Vector2 start = new(0f, -92f);
            Vector2 size = new(474f, 58f);
            for (int i = 0; i < RowsPerPage; i++)
            {
                AchievementRowView row = AchievementRowView.Create(contentRoot, start + new Vector2(0f, -i * 64f), size, this);
                rowViews.Add(row);
            }
        }

        private void CreatePageControls()
        {
            previousPageButton = CreateButton(contentRoot, "上一页", new Vector2(306f, -8f), new Vector2(52f, 26f), 12);
            previousPageButton.onClick.AddListener(() => ChangePage(currentPage - 1));

            pageText = CreateText(contentRoot, "PageText", new Vector2(364f, -8f), new Vector2(42f, 26f), 12, TextAnchor.MiddleCenter);

            nextPageButton = CreateButton(contentRoot, "下一页", new Vector2(412f, -8f), new Vector2(52f, 26f), 12);
            nextPageButton.onClick.AddListener(() => ChangePage(currentPage + 1));
        }

        private void ChangePage(int nextPage)
        {
            currentPage = Mathf.Clamp(nextPage, 0, GetMaxPage());
            Refresh();
        }

        private void Refresh()
        {
            if (achievementService == null)
            {
                achievementService = new AchievementService();
            }

            List<AchievementViewModel> achievements = achievementService.GetAchievements(selectedCategory);
            int maxPage = GetMaxPage(achievements.Count);
            currentPage = Mathf.Clamp(currentPage, 0, maxPage);
            int firstIndex = currentPage * RowsPerPage;

            int unlocked = achievementService.GetUnlockedCount(selectedCategory);
            int total = AchievementDatabase.GetCategoryTotal(selectedCategory);
            int claimed = achievementService.GetClaimedCount(selectedCategory);
            statusText.text = $"{AchievementDatabase.GetCategoryDisplayName(selectedCategory)}成就  已达成 {unlocked}/{total}  已领取 {claimed}/{total}";

            RefreshCategoryButtons();

            for (int i = 0; i < rowViews.Count; i++)
            {
                int index = firstIndex + i;
                rowViews[i].Refresh(index < achievements.Count ? achievements[index] : null);
            }

            pageText.text = $"{currentPage + 1}/{maxPage + 1}";
            previousPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < maxPage;
        }

        private void RefreshCategoryButtons()
        {
            for (int i = 0; i < categoryButtons.Count; i++)
            {
                Image image = categoryButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    image.color = categories[i] == selectedCategory
                        ? new Color(0.98f, 0.82f, 0.46f, 1f)
                        : new Color(1f, 0.95f, 0.84f, 0.96f);
                }
            }
        }

        private bool TryClaim(AchievementViewModel viewModel, AchievementRowView row)
        {
            if (viewModel == null)
            {
                return false;
            }

            bool claimed = achievementService.TryClaim(viewModel.definition.id, out string feedback);
            if (claimed)
            {
                row.ShowFeedback(feedback);
                Refresh();
            }

            return claimed;
        }

        private int GetMaxPage()
        {
            return GetMaxPage(achievementService.GetAchievements(selectedCategory).Count);
        }

        private static int GetMaxPage(int count)
        {
            return Mathf.Max(0, Mathf.CeilToInt(count / (float)RowsPerPage) - 1);
        }

        private static string BuildRewardText(AchievementReward reward)
        {
            if (reward.IsEmpty)
            {
                return "奖励：无";
            }

            List<string> parts = new();
            if (reward.intimacy != 0)
            {
                parts.Add($"亲密度 +{reward.intimacy}");
            }

            if (reward.energyMax > 0f)
            {
                parts.Add($"活力上限 +{reward.energyMax:0}");
            }

            return "奖励：" + string.Join("  ", parts);
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

        private class AchievementRowView
        {
            private readonly AchievementWindowController controller;
            private readonly Transform root;
            private readonly Text label;
            private readonly Button claimButton;
            private readonly Text claimButtonText;
            private readonly Image background;
            private AchievementViewModel viewModel;

            private AchievementRowView(AchievementWindowController controller, Transform root, Text label, Button claimButton, Image background)
            {
                this.controller = controller;
                this.root = root;
                this.label = label;
                this.claimButton = claimButton;
                this.claimButtonText = claimButton.GetComponentInChildren<Text>();
                this.background = background;
                this.claimButton.onClick.AddListener(OnClaimClicked);
            }

            public static AchievementRowView Create(Transform parent, Vector2 position, Vector2 size, AchievementWindowController controller)
            {
                GameObject root = new("AchievementRow", typeof(RectTransform), typeof(Image));
                root.transform.SetParent(parent, false);

                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;

                Image background = root.GetComponent<Image>();
                background.color = new Color(1f, 0.98f, 0.9f, 0.9f);

                Text label = CreateText(root.transform, "AchievementLabel", new Vector2(10f, -5f), new Vector2(size.x - 104f, size.y - 8f), 12, TextAnchor.UpperLeft);
                Button claimButton = CreateButton(root.transform, "领取", new Vector2(size.x - 82f, -15f), new Vector2(70f, 28f), 12);
                return new AchievementRowView(controller, root.transform, label, claimButton, background);
            }

            public void Refresh(AchievementViewModel nextViewModel)
            {
                viewModel = nextViewModel;
                root.gameObject.SetActive(viewModel != null);
                if (viewModel == null)
                {
                    return;
                }

                AchievementDefinition definition = viewModel.definition;
                string state = viewModel.claimed ? "已领取" : viewModel.unlocked ? "可领取" : "未达成";
                label.text =
                    $"{definition.displayName}  [{state}]\n" +
                    $"{definition.conditionText}  进度：{viewModel.progressText}\n" +
                    BuildRewardText(definition.reward);

                background.color = viewModel.claimed
                    ? new Color(0.86f, 0.92f, 0.84f, 0.92f)
                    : viewModel.unlocked
                        ? new Color(1f, 0.94f, 0.72f, 0.96f)
                        : new Color(1f, 0.98f, 0.9f, 0.9f);

                claimButton.interactable = viewModel.unlocked && !viewModel.claimed;
                if (claimButtonText != null)
                {
                    claimButtonText.text = viewModel.claimed ? "已领" : viewModel.unlocked ? "领取" : "未达成";
                }
            }

            public void ShowFeedback(string message)
            {
                FloatingTextFeedback.Show(root, message, new Vector2(0f, 4f), new Color(0.18f, 0.55f, 0.22f, 1f));
            }

            private void OnClaimClicked()
            {
                controller.TryClaim(viewModel, this);
            }
        }
    }
}
