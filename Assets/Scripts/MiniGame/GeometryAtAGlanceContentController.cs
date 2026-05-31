using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public partial class GeometryAtAGlanceContentController : MiniGameWindowContentController
    {
        private const string BestScoreKey = "MiniGame.GeometryAtAGlance.BestScore";
        private const float InitialMemorizeTime = 6.8f;
        private const float MinimumMemorizeTime = 1.8f;
        private const float MemorizeTimeDecayPerLevel = 0.26f;
        private const int ScorePerRound = 100;

        private readonly List<Button> cellButtons = new();
        private readonly List<Text> cellLabels = new();
        private readonly List<Image> cellImages = new();
        private readonly HashSet<int> selectedIndices = new();
        private readonly HashSet<int> changedIndices = new();

        private readonly GeometryShapeSpec[] shapes =
        {
            new("\u25CB", "\u5706"),
            new("\u25A1", "\u65b9"),
            new("\u25B3", "\u4e09"),
            new("\u25C7", "\u83f1")
        };

        private readonly Color[] palette =
        {
            new(0.95f, 0.43f, 0.40f),
            new(0.31f, 0.58f, 0.94f),
            new(0.97f, 0.76f, 0.28f),
            new(0.41f, 0.74f, 0.49f),
            new(0.75f, 0.53f, 0.89f)
        };

        private Text instructionText;
        private Text phaseText;
        private Text statusText;
        private Text resultText;
        private Text bestText;
        private Button backButton;
        private Button actionButton;
        private Button submitButton;
        private GridLayoutGroup grid;

        private GeometryCellSpec[] basePattern;
        private GeometryCellSpec[] shownPattern;
        private float phaseTimer;
        private int level = 1;
        private int bestScore;
        private int gridSize = 3;
        private GeometryPhase currentPhase = GeometryPhase.Idle;
        private int completedRoundsThisSession;
        private bool sessionBrokeRecord;
        private bool rewardApplied;

        protected override void BuildContent()
        {
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            rewardApplied = false;

            instructionText = MiniGameUiFactory.CreateText("InstructionText", ContentRoot, 18, TextAnchor.UpperLeft, new Color(0.24f, 0.24f, 0.24f));
            instructionText.text = "\u5148\u89c2\u5bdf\u56fe\u5f62\u9635\u5217\uff0c\u518d\u627e\u51fa\u53d8\u5316\u7684\u683c\u5b50\u3002";
            MiniGameUiFactory.SetAnchors(instructionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, 0f));

            Image topBar = MiniGameUiFactory.CreatePanel("TopBar", ContentRoot, new Color(1f, 1f, 1f, 0.82f));
            MiniGameUiFactory.SetAnchors(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -84f), new Vector2(0f, -42f));

            phaseText = MiniGameUiFactory.CreateText("PhaseText", topBar.transform, 21, TextAnchor.UpperLeft, new Color(0.15f, 0.15f, 0.15f));
            MiniGameUiFactory.SetAnchors(phaseText.rectTransform, new Vector2(0f, 0.48f), new Vector2(1f, 1f), new Vector2(12f, 0f), new Vector2(-12f, 0f));
            statusText = MiniGameUiFactory.CreateText("StatusText", topBar.transform, 17, TextAnchor.LowerLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.52f), new Vector2(12f, 0f), new Vector2(-12f, 0f));

            RectTransform gridRoot = MiniGameUiFactory.CreateRect("GridRoot", ContentRoot);
            MiniGameUiFactory.SetAnchors(gridRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 70f), new Vector2(-16f, -92f));
            grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(84f, 44f);
            grid.spacing = new Vector2(8f, 8f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            CreateGridCells(gridRoot, 9);

            resultText = MiniGameUiFactory.CreateText("ResultText", ContentRoot, 17, TextAnchor.MiddleCenter, new Color(0.3f, 0.3f, 0.3f));
            MiniGameUiFactory.SetAnchors(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 42f), new Vector2(-10f, 66f));

            Image footer = MiniGameUiFactory.CreatePanel("Footer", ContentRoot, new Color(1f, 1f, 1f, 0.72f));
            MiniGameUiFactory.SetAnchors(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 34f));

            backButton = CreateBackButton(footer.transform, new Vector2(0f, 0f), new Vector2(0.20f, 1f));

            bestText = MiniGameUiFactory.CreateText("BestText", footer.transform, 17, TextAnchor.MiddleLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.22f, 0f), new Vector2(0.36f, 1f), new Vector2(10f, 0f), Vector2.zero);

            submitButton = MiniGameUiFactory.CreateButton("SubmitButton", footer.transform, "\u63d0\u4ea4\u7b54\u6848", new Color(0.86f, 0.92f, 0.99f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(submitButton.GetComponent<RectTransform>(), new Vector2(0.40f, 0f), new Vector2(0.66f, 1f), Vector2.zero, new Vector2(0f, -4f));
            submitButton.onClick.AddListener(SubmitAnswer);

            actionButton = MiniGameUiFactory.CreateButton("ActionButton", footer.transform, "\u5f00\u59cb\u89c2\u5bdf", new Color(0.95f, 0.86f, 0.72f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(actionButton.GetComponent<RectTransform>(), new Vector2(0.70f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-10f, -4f));
            actionButton.onClick.AddListener(HandleActionButton);

            SetCellsInteractable(false);
            submitButton.interactable = false;
            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
        }

        private void Update()
        {
            if (currentPhase != GeometryPhase.Memorize)
            {
                return;
            }

            phaseTimer -= Time.deltaTime;
            if (phaseTimer <= 0f)
            {
                BeginAnswerPhase();
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
            if (currentPhase == GeometryPhase.Memorize || currentPhase == GeometryPhase.Answer)
            {
                currentPhase = GeometryPhase.ResultLose;
                ApplySessionRewards();
            }

            RefreshMiniGameAvailability(resultText, actionButton);
        }

        protected override void ResetRuntimeState()
        {
            cellButtons.Clear();
            cellLabels.Clear();
            cellImages.Clear();
            selectedIndices.Clear();
            changedIndices.Clear();
            instructionText = null;
            phaseText = null;
            statusText = null;
            resultText = null;
            bestText = null;
            backButton = null;
            actionButton = null;
            submitButton = null;
            grid = null;
            basePattern = null;
            shownPattern = null;
        }

        private void HandleActionButton()
        {
            switch (currentPhase)
            {
                case GeometryPhase.Idle:
                case GeometryPhase.ResultLose:
                    if (!TryBeginMiniGameSession(resultText))
                    {
                        UpdateTexts();
                        RefreshMiniGameAvailability(resultText, actionButton);
                        return;
                    }

                    level = Mathf.Max(1, level);
                    completedRoundsThisSession = 0;
                    sessionBrokeRecord = false;
                    rewardApplied = false;
                    StartRound();
                    break;
                case GeometryPhase.ResultWin:
                    level++;
                    StartRound();
                    break;
            }
        }

        private void StartRound()
        {
            currentPhase = GeometryPhase.Memorize;
            selectedIndices.Clear();
            changedIndices.Clear();

            gridSize = level >= 6 ? 4 : 3;
            int totalCells = gridSize * gridSize;
            EnsureGridCellCount(totalCells);
            grid.constraintCount = gridSize;
            grid.cellSize = gridSize == 3 ? new Vector2(82f, 64f) : new Vector2(60f, 52f);

            basePattern = new GeometryCellSpec[totalCells];
            shownPattern = new GeometryCellSpec[totalCells];

            for (int index = 0; index < totalCells; index++)
            {
                basePattern[index] = RandomCell();
                shownPattern[index] = basePattern[index];
            }

            int changeCount = level <= 10 ? 1 : Mathf.Clamp(1 + (level - 9) / 2, 1, Mathf.Max(2, totalCells / 3));
            while (changedIndices.Count < changeCount)
            {
                changedIndices.Add(Random.Range(0, totalCells));
            }

            foreach (int changedIndex in changedIndices)
            {
                shownPattern[changedIndex] = RandomDifferentCell(basePattern[changedIndex]);
            }

            ApplyPattern(basePattern, revealSelections: false, showChangeHints: false);
            SetCellsInteractable(false);
            submitButton.interactable = false;
            phaseTimer = GetMemorizeDuration();
            actionButton.GetComponentInChildren<Text>().text = "\u4e0b\u4e00\u8f6e";
            resultText.text = "\u89c2\u5bdf\u4e2d...";
            UpdateTexts();
        }

        private void BeginAnswerPhase()
        {
            currentPhase = GeometryPhase.Answer;
            ApplyPattern(shownPattern, revealSelections: true, showChangeHints: false);
            SetCellsInteractable(true);
            submitButton.interactable = true;
            resultText.text = "\u9009\u51fa\u4f60\u8ba4\u4e3a\u53d1\u751f\u53d8\u5316\u7684\u683c\u5b50\u3002";
            UpdateTexts();
        }

        private void SubmitAnswer()
        {
            if (currentPhase != GeometryPhase.Answer)
            {
                return;
            }

            submitButton.interactable = false;
            SetCellsInteractable(false);

            bool success = selectedIndices.SetEquals(changedIndices);
            if (success)
            {
                currentPhase = GeometryPhase.ResultWin;
                completedRoundsThisSession++;
                int adjustedScore = ApplySessionScoreModifier(GetSessionScore(), out _);
                if (adjustedScore > bestScore)
                {
                    bestScore = adjustedScore;
                    sessionBrokeRecord = true;
                    PlayerPrefs.SetInt(BestScoreKey, bestScore);
                    PlayerPrefs.Save();
                }

                resultText.text = "\u5224\u65ad\u6b63\u786e\uff0c\u8fdb\u5165\u4e0b\u4e00\u5173\u3002";
            }
            else
            {
                currentPhase = GeometryPhase.ResultLose;
                resultText.text = "\u6709\u9057\u6f0f\u6216\u8bef\u9009\uff0c\u70b9\u51fb\u518d\u8bd5\u4e00\u5173\u3002";
                ApplySessionRewards();
            }

            ApplyPattern(shownPattern, revealSelections: true, showChangeHints: true);
            UpdateTexts();
        }

        private void ToggleSelection(int index)
        {
            if (currentPhase != GeometryPhase.Answer)
            {
                return;
            }

            if (selectedIndices.Contains(index))
            {
                selectedIndices.Remove(index);
            }
            else
            {
                selectedIndices.Add(index);
            }

            UpdateSelectionState(index);
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            if (phaseText == null || statusText == null || bestText == null || actionButton == null || submitButton == null)
            {
                return;
            }

            string phaseLabel = currentPhase switch
            {
                GeometryPhase.Memorize => $"\u89c2\u5bdf\u4e2d: {Mathf.Max(phaseTimer, 0f):0.0}s",
                GeometryPhase.Answer => "\u4f5c\u7b54\u9636\u6bb5",
                GeometryPhase.ResultWin => "\u672c\u8f6e\u5b8c\u6210",
                GeometryPhase.ResultLose => "\u672c\u8f6e\u5931\u8d25",
                _ => "\u51c6\u5907\u5f00\u59cb"
            };

            phaseText.text = phaseLabel;
            int adjustedScore = ApplySessionScoreModifier(GetSessionScore(), out _);
            statusText.text = $"\u5f97\u5206 {adjustedScore}   \u5bab\u683c {gridSize}x{gridSize}   \u5df2\u9009 {selectedIndices.Count}";
            bestText.text = $"\u5386\u53f2\u6700\u4f73: {bestScore}";

            submitButton.gameObject.SetActive(currentPhase == GeometryPhase.Answer);
            actionButton.GetComponentInChildren<Text>().text = currentPhase switch
            {
                GeometryPhase.ResultWin => "\u4e0b\u4e00\u5173",
                GeometryPhase.ResultLose => "\u518d\u8bd5\u4e00\u5173",
                GeometryPhase.Idle => "\u5f00\u59cb\u89c2\u5bdf",
                _ => "\u4e0b\u4e00\u8f6e"
            };
        }

    }
}
