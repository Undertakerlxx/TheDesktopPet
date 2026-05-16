using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public class ColorGridContentController : MiniGameWindowContentController
    {
        private const string BestLevelKey = "MiniGame.ColorGrid.BestLevel";
        private const int SuccessScoreThreshold = 10;

        private Text instructionText;
        private Text statusText;
        private Text resultText;
        private Text bestText;
        private Button backButton;
        private Button actionButton;

        private GridLayoutGroup grid;
        private readonly List<Button> cells = new();
        private readonly List<Image> cellImages = new();

        private int level = 1;
        private int bestLevel = 1;
        private int targetIndex = -1;
        private int gridSize = 3;
        private float roundTime;
        private bool isPlaying;
        private int sessionScore;
        private bool sessionBrokeRecord;
        private bool rewardApplied;

        protected override void ConfigureHostWindow()
        {
            base.ConfigureHostWindow();
            SetWindowSize(StandardWindowWidth, StandardWindowHeight);
        }

        protected override void BuildContent()
        {
            bestLevel = PlayerPrefs.GetInt(BestLevelKey, 1);
            rewardApplied = false;

            instructionText = MiniGameUiFactory.CreateText("InstructionText", ContentRoot, 18, TextAnchor.UpperLeft, new Color(0.24f, 0.24f, 0.24f));
            instructionText.text = "\u627e\u51fa\u989c\u8272\u7565\u6709\u4e0d\u540c\u7684\u90a3\u4e2a\u65b9\u5757\uff0c\u5173\u5361\u8d8a\u9ad8\u5dee\u5f02\u8d8a\u5c0f\u3002";
            MiniGameUiFactory.SetAnchors(instructionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            Image statusPanel = MiniGameUiFactory.CreatePanel("StatusPanel", ContentRoot, new Color(1f, 1f, 1f, 0.82f));
            MiniGameUiFactory.SetAnchors(statusPanel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -80f), new Vector2(0f, -42f));

            statusText = MiniGameUiFactory.CreateText("StatusText", statusPanel.transform, 18, TextAnchor.MiddleLeft, new Color(0.2f, 0.2f, 0.2f));
            MiniGameUiFactory.SetAnchors(statusText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

            RectTransform gridRoot = MiniGameUiFactory.CreateRect("GridRoot", ContentRoot);
            MiniGameUiFactory.SetAnchors(gridRoot, Vector2.zero, Vector2.one, new Vector2(16f, 70f), new Vector2(-16f, -92f));

            grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(84f, 44f);
            grid.spacing = new Vector2(8f, 8f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            CreateGridCells(gridRoot, 25);

            resultText = MiniGameUiFactory.CreateText("ResultText", ContentRoot, 17, TextAnchor.MiddleCenter, new Color(0.3f, 0.3f, 0.3f));
            MiniGameUiFactory.SetAnchors(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 42f), new Vector2(-10f, 66f));

            Image footer = MiniGameUiFactory.CreatePanel("Footer", ContentRoot, new Color(1f, 1f, 1f, 0.72f));
            MiniGameUiFactory.SetAnchors(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 34f));

            backButton = CreateBackButton(footer.transform, new Vector2(0f, 0f), new Vector2(0.20f, 1f));

            bestText = MiniGameUiFactory.CreateText("BestText", footer.transform, 17, TextAnchor.MiddleLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.22f, 0f), new Vector2(0.44f, 1f), new Vector2(10f, 0f), Vector2.zero);

            actionButton = MiniGameUiFactory.CreateButton("ActionButton", footer.transform, "\u5f00\u59cb\u627e\u8272", new Color(0.95f, 0.86f, 0.72f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(actionButton.GetComponent<RectTransform>(), new Vector2(0.60f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-10f, -4f));
            actionButton.onClick.AddListener(StartGame);

            ShowIdleState();
            UpdateTexts();
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            roundTime -= Time.deltaTime;
            if (roundTime <= 0f)
            {
                roundTime = 0f;
                isPlaying = false;
                resultText.text = "\u65f6\u95f4\u5230\uff0c\u70b9\u51fb\u91cd\u65b0\u5f00\u59cb\u3002";
                ApplySessionRewards();
            }

            UpdateTexts();
        }

        protected override void ResetRuntimeState()
        {
            instructionText = null;
            statusText = null;
            resultText = null;
            bestText = null;
            backButton = null;
            actionButton = null;
            grid = null;
            cells.Clear();
            cellImages.Clear();
        }

        protected override void RefreshView()
        {
            UpdateTexts();
        }

        public override void HandleWindowClosed()
        {
            isPlaying = false;
            ShowIdleState();
            UpdateTexts();
        }

        private void StartGame()
        {
            if (!isPlaying)
            {
                level = Mathf.Max(1, level);
                sessionScore = 0;
                sessionBrokeRecord = false;
                rewardApplied = false;
            }

            isPlaying = true;
            roundTime = Mathf.Max(3f, 7f - (level * 0.18f));
            BuildRound();
            actionButton.GetComponentInChildren<Text>().text = "\u91cd\u65b0\u5f00\u59cb";
            resultText.text = "\u627e\u51fa\u989c\u8272\u4e0d\u4e00\u6837\u7684\u90a3\u4e00\u5757\u3002";
            UpdateTexts();
        }

        private void BuildRound()
        {
            gridSize = Mathf.Clamp(3 + (level - 1) / 4, 3, 5);
            grid.constraintCount = gridSize;
            grid.cellSize = gridSize switch
            {
                3 => new Vector2(84f, 44f),
                4 => new Vector2(62f, 38f),
                _ => new Vector2(48f, 30f)
            };

            int totalCells = gridSize * gridSize;
            EnsureVisibleCellCount(totalCells);

            float hue = Random.value;
            Color baseColor = Color.HSVToRGB(hue, 0.55f, 0.92f);
            float delta = Mathf.Max(0.06f, 0.20f - (level * 0.008f));
            Color targetColor = Color.HSVToRGB(hue, 0.55f, Mathf.Clamp01(0.92f - delta));

            targetIndex = Random.Range(0, totalCells);
            for (int index = 0; index < totalCells; index++)
            {
                cellImages[index].color = index == targetIndex ? targetColor : baseColor;
                cells[index].interactable = true;
            }
        }

        private void HandleCellClicked(int index)
        {
            if (!isPlaying || index < 0 || index >= cells.Count)
            {
                return;
            }

            if (index == targetIndex)
            {
                level++;
                sessionScore++;
                if (level > bestLevel)
                {
                    bestLevel = level;
                    sessionBrokeRecord = true;
                    PlayerPrefs.SetInt(BestLevelKey, bestLevel);
                    PlayerPrefs.Save();
                }

                resultText.text = "\u627e\u5230\u4e86\uff0c\u8fdb\u5165\u4e0b\u4e00\u5173\u3002";
                StartGame();
                return;
            }

            isPlaying = false;
            resultText.text = "\u70b9\u9519\u4e86\uff0c\u989c\u8272\u5dee\u5f02\u5e76\u4e0d\u5728\u8fd9\u91cc\u3002";
            ApplySessionRewards();
            UpdateTexts();
        }

        private void ShowIdleState()
        {
            EnsureVisibleCellCount(9);
            foreach (Image image in cellImages)
            {
                image.color = new Color(0.84f, 0.89f, 0.97f, 0.95f);
            }

            foreach (Button button in cells)
            {
                button.interactable = false;
            }
        }

        private void EnsureVisibleCellCount(int totalCells)
        {
            while (cells.Count < totalCells)
            {
                int capturedIndex = cells.Count;
                Button button = MiniGameUiFactory.CreateButton($"Cell{capturedIndex}", grid.transform, string.Empty, new Color(1f, 1f, 1f, 0.95f), new Color(0.2f, 0.2f, 0.2f));
                button.onClick.AddListener(() => HandleCellClicked(capturedIndex));
                cells.Add(button);
                cellImages.Add(button.GetComponent<Image>());
                button.GetComponentInChildren<Text>().text = string.Empty;
            }

            for (int index = 0; index < cells.Count; index++)
            {
                bool active = index < totalCells;
                cells[index].gameObject.SetActive(active);
            }
        }

        private void CreateGridCells(Transform parent, int count)
        {
            for (int index = 0; index < count; index++)
            {
                Button button = MiniGameUiFactory.CreateButton($"Cell{index}", parent, string.Empty, new Color(1f, 1f, 1f, 0.95f), new Color(0.2f, 0.2f, 0.2f));
                button.GetComponentInChildren<Text>().text = string.Empty;
                int capturedIndex = index;
                button.onClick.AddListener(() => HandleCellClicked(capturedIndex));
                cells.Add(button);
                cellImages.Add(button.GetComponent<Image>());
            }
        }

        private void UpdateTexts()
        {
            if (statusText == null || bestText == null)
            {
                return;
            }

            statusText.text = isPlaying
                ? $"\u5173\u5361: {level}   \u5bab\u683c: {gridSize}x{gridSize}   \u5269\u4f59: {roundTime:0.0}s"
                : $"\u5173\u5361: {level}   \u70b9\u51fb\u5f00\u59cb\u8fdb\u5165\u6311\u6218";
            bestText.text = $"\u6700\u9ad8\u5173\u5361: {bestLevel}";
        }

        private void ApplySessionRewards()
        {
            if (rewardApplied)
            {
                return;
            }

            ApplyMiniGameResult(MiniGameKind.ColorGrid, sessionScore >= SuccessScoreThreshold, sessionBrokeRecord, sessionScore);
            rewardApplied = true;
        }
    }
}
