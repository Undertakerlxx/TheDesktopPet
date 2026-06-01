using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public partial class EyeHandSpeedContentController : MiniGameWindowContentController
    {
        private const float TotalRoundTime = 30f;
        private const int ScorePerHit = 100;
        private const int SuccessScoreThreshold = 1000;
        protected override MiniGameKind ControlledGameKind => MiniGameKind.EyeHandSpeed;

        private readonly List<Button> cellButtons = new();
        private readonly List<Text> cellLabels = new();
        private readonly List<Image> cellImages = new();

        private readonly EyeHandShapeSpec[] shapes =
        {
            new("\u25CB", "\u5706\u5f62"),
            new("\u25A1", "\u65b9\u5f62"),
            new("\u25B3", "\u4e09\u89d2"),
            new("\u25C7", "\u83f1\u5f62")
        };

        private readonly EyeHandNamedColor[] palette =
        {
            new(new Color(0.94f, 0.42f, 0.38f), "\u7ea2\u8272"),
            new(new Color(0.29f, 0.56f, 0.93f), "\u84dd\u8272"),
            new(new Color(0.97f, 0.75f, 0.26f), "\u9ec4\u8272"),
            new(new Color(0.38f, 0.72f, 0.45f), "\u7eff\u8272")
        };

        private Text instructionText;
        private Text promptText;
        private Text statusText;
        private Text resultText;
        private Text bestText;
        private Button backButton;
        private Button actionButton;

        private EyeHandTileData[] currentTiles;
        private EyeHandTargetSpec currentTarget;
        private bool isPlaying;
        private float remainingTime;
        private float roundRemaining;
        private int score;
        private int lives;
        private int bestScore;
        private bool rewardApplied;

        protected override void BuildContent()
        {
            bestScore = LoadStoredBestScore();
            rewardApplied = false;

            instructionText = MiniGameUiFactory.CreateText("InstructionText", ContentRoot, 18, TextAnchor.UpperLeft, new Color(0.24f, 0.24f, 0.24f));
            instructionText.text = "\u6309\u63d0\u793a\u70b9\u51fb\u76ee\u6807\uff0c\u907f\u5f00\u70b8\u5f39\u548c\u5e72\u6270\u9879\u3002";
            MiniGameUiFactory.SetAnchors(instructionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, 0f));

            Image topBar = MiniGameUiFactory.CreatePanel("TopBar", ContentRoot, new Color(1f, 1f, 1f, 0.82f));
            MiniGameUiFactory.SetAnchors(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -84f), new Vector2(0f, -42f));

            promptText = MiniGameUiFactory.CreateText("PromptText", topBar.transform, 22, TextAnchor.MiddleLeft, new Color(0.15f, 0.15f, 0.15f));
            promptText.text = "\u70b9\u51fb\u5f00\u59cb";
            MiniGameUiFactory.SetAnchors(promptText.rectTransform, new Vector2(0f, 0f), new Vector2(0.58f, 1f), new Vector2(12f, 0f), new Vector2(-4f, 0f));

            statusText = MiniGameUiFactory.CreateText("StatusText", topBar.transform, 18, TextAnchor.MiddleRight, new Color(0.2f, 0.2f, 0.2f));
            MiniGameUiFactory.SetAnchors(statusText.rectTransform, new Vector2(0.58f, 0f), new Vector2(1f, 1f), new Vector2(4f, 0f), new Vector2(-12f, 0f));

            RectTransform gridRoot = MiniGameUiFactory.CreateRect("GridRoot", ContentRoot);
            MiniGameUiFactory.SetAnchors(gridRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 70f), new Vector2(-16f, -92f));

            GridLayoutGroup grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(92f, 44f);
            grid.spacing = new Vector2(8f, 8f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            for (int index = 0; index < 9; index++)
            {
                Button button = MiniGameUiFactory.CreateButton($"Cell{index}", gridRoot, "", new Color(1f, 1f, 1f, 0.95f), new Color(0.2f, 0.2f, 0.2f));
                button.onClick.AddListener(() => HandleCellClicked(button));
                cellButtons.Add(button);
                cellImages.Add(button.GetComponent<Image>());
                Text label = button.GetComponentInChildren<Text>();
                MiniGameUiFactory.StyleSymbolText(label, 34, new Color(0.10f, 0.10f, 0.10f), new Color(1f, 1f, 1f, 0.55f));
                cellLabels.Add(label);
            }

            resultText = MiniGameUiFactory.CreateText("ResultText", ContentRoot, 17, TextAnchor.MiddleCenter, new Color(0.3f, 0.3f, 0.3f));
            MiniGameUiFactory.SetAnchors(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 42f), new Vector2(-10f, 66f));

            Image footer = MiniGameUiFactory.CreatePanel("Footer", ContentRoot, new Color(1f, 1f, 1f, 0.72f));
            MiniGameUiFactory.SetAnchors(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 34f));

            backButton = CreateBackButton(footer.transform, new Vector2(0f, 0f), new Vector2(0.20f, 1f));

            bestText = MiniGameUiFactory.CreateText("BestText", footer.transform, 17, TextAnchor.MiddleLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.22f, 0f), new Vector2(0.54f, 1f), new Vector2(10f, 0f), Vector2.zero);

            actionButton = MiniGameUiFactory.CreateButton("ActionButton", footer.transform, "\u5f00\u59cb\u6311\u6218", new Color(0.95f, 0.86f, 0.72f, 0.95f), new Color(0.2f, 0.2f, 0.2f));
            MiniGameUiFactory.SetAnchors(actionButton.GetComponent<RectTransform>(), new Vector2(0.66f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-10f, -4f));
            actionButton.onClick.AddListener(StartGame);

            SetGridInteractable(false);
            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            remainingTime -= Time.deltaTime;
            roundRemaining -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                FinishGame("\u65f6\u95f4\u5230\uff0c\u6311\u6218\u7ed3\u675f\u3002");
                return;
            }

            if (roundRemaining <= 0f)
            {
                roundRemaining = 0f;
                ApplyMistake("\u53cd\u5e94\u6162\u4e86\u4e00\u6b65\u3002");
            }

            UpdateTexts();
        }

        protected override void RefreshView()
        {
            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
        }

        public override void HandleWindowClosed()
        {
            if (isPlaying)
            {
                FinishGame("\u672c\u5c40\u63d0\u524d\u7ed3\u675f\u3002");
            }

            RefreshMiniGameAvailability(resultText, actionButton);
        }

        protected override void ResetRuntimeState()
        {
            cellButtons.Clear();
            cellLabels.Clear();
            cellImages.Clear();
            instructionText = null;
            promptText = null;
            statusText = null;
            resultText = null;
            bestText = null;
            backButton = null;
            actionButton = null;
            currentTiles = null;
        }

        private void StartGame()
        {
            if (!TryBeginMiniGameSession(resultText))
            {
                UpdateTexts();
                RefreshMiniGameAvailability(resultText, actionButton);
                return;
            }

            isPlaying = true;
            rewardApplied = false;
            remainingTime = TotalRoundTime;
            score = 0;
            lives = 3;
            resultText.text = "\u4fdd\u6301\u4e13\u6ce8\uff0c\u8d8a\u5230\u540e\u9762\u8d8a\u5feb\u3002";
            actionButton.GetComponentInChildren<Text>().text = "\u91cd\u65b0\u5f00\u59cb";
            SetGridInteractable(true);
            GenerateRound();
            UpdateTexts();
        }

        private void GenerateRound()
        {
            currentTarget = RandomTarget();
            int clearedTargets = score / ScorePerHit;
            int bombCount = Mathf.Clamp(1 + clearedTargets / 4, 1, 3);
            currentTiles = new EyeHandTileData[cellButtons.Count];

            List<int> availableIndices = new();
            for (int index = 0; index < currentTiles.Length; index++)
            {
                availableIndices.Add(index);
            }

            int targetIndex = TakeRandomIndex(availableIndices);
            currentTiles[targetIndex] = EyeHandTileData.Target(currentTarget);

            for (int bombIndex = 0; bombIndex < bombCount; bombIndex++)
            {
                int index = TakeRandomIndex(availableIndices);
                currentTiles[index] = EyeHandTileData.Bomb();
            }

            foreach (int index in availableIndices)
            {
                currentTiles[index] = EyeHandTileData.Decoy(RandomDecoy(currentTarget));
            }

            for (int index = 0; index < currentTiles.Length; index++)
            {
                ApplyTileVisual(index, currentTiles[index]);
            }

            roundRemaining = Mathf.Max(0.85f, 2.2f - (clearedTargets * 0.06f));
            promptText.text = $"\u70b9\u51fb: {currentTarget.ColorName}{currentTarget.ShapeName}";
        }

        private void HandleCellClicked(Button button)
        {
            if (!isPlaying)
            {
                return;
            }

            int index = cellButtons.IndexOf(button);
            if (index < 0 || currentTiles == null || index >= currentTiles.Length)
            {
                return;
            }

            EyeHandTileData tileData = currentTiles[index];
            switch (tileData.Role)
            {
                case EyeHandTileRole.Target:
                    score += ScorePerHit;
                    resultText.text = "\u547d\u4e2d\u76ee\u6807\u3002";
                    GenerateRound();
                    break;
                case EyeHandTileRole.Bomb:
                    ApplyMistake("\u8e29\u5230\u70b8\u5f39\u4e86\u3002");
                    break;
                default:
                    ApplyMistake("\u70b9\u5230\u4e86\u5e72\u6270\u9879\u3002");
                    break;
            }

            UpdateTexts();
        }

        private void ApplyMistake(string message)
        {
            lives--;
            resultText.text = message;
            if (lives <= 0)
            {
                FinishGame("\u673a\u4f1a\u7528\u5b8c\uff0c\u6311\u6218\u7ed3\u675f\u3002");
                return;
            }

            GenerateRound();
        }

        private void FinishGame(string message)
        {
            isPlaying = false;
            SetGridInteractable(false);
            int adjustedScore = ApplySessionScoreModifier(score, out _);
            bool brokeRecord = adjustedScore > bestScore;
            if (adjustedScore > bestScore)
            {
                bestScore = adjustedScore;
                SaveStoredBestScore(bestScore);
                message = $"{message} \u5237\u65b0\u4e86\u6700\u4f73\u6210\u7ee9\u3002";
            }

            string scoreBreakdown = FormatSessionModifierBreakdown(score, adjustedScore);
            string modifierSuffix = string.IsNullOrEmpty(SessionScoreModifierLabel) ? string.Empty : $" ({SessionScoreModifierLabel})";
            resultText.text = $"{message} \u672c\u6b21\u5f97\u5206 {scoreBreakdown}{modifierSuffix}\u3002";
            promptText.text = "\u70b9\u51fb\u5f00\u59cb";
            actionButton.GetComponentInChildren<Text>().text = "\u518d\u73a9\u4e00\u6b21";

            if (!rewardApplied)
            {
                ApplyMiniGameResult(MiniGameKind.EyeHandSpeed, adjustedScore >= SuccessScoreThreshold, brokeRecord, adjustedScore);
                rewardApplied = true;
            }

            UpdateTexts();
        }

        private void UpdateTexts()
        {
            if (statusText == null || bestText == null)
            {
                return;
            }

            int adjustedScore = ApplySessionScoreModifier(score, out _);
            statusText.text = $"\u5f97\u5206 {adjustedScore}   \u751f\u547d {Mathf.Max(lives, 0)}   \u65f6\u95f4 {remainingTime:0.0}s";
            bestText.text = $"\u5386\u53f2\u6700\u4f73: {bestScore}";
        }

        private void SetGridInteractable(bool interactable)
        {
            foreach (Button button in cellButtons)
            {
                button.interactable = interactable;
            }
        }

    }
}
