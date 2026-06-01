using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public class SchulteGridContentController : MiniGameWindowContentController
    {
        protected override MiniGameKind ControlledGameKind => MiniGameKind.SchulteGrid;

        private readonly List<Button> numberButtons = new();
        private readonly List<Text> numberLabels = new();
        private readonly List<Image> numberImages = new();

        private Text instructionText;
        private Text statusText;
        private Text resultText;
        private Text bestText;
        private Button backButton;
        private Button actionButton;

        private int nextNumber;
        private float elapsedTime;
        private float bestTime;
        private bool isPlaying;
        private bool rewardApplied;

        protected override void ConfigureHostWindow()
        {
            base.ConfigureHostWindow();
            SetWindowSize(StandardWindowWidth, StandardWindowHeight);
        }

        protected override void BuildContent()
        {
            bestTime = LoadStoredBestFloat();
            rewardApplied = false;

            instructionText = MiniGameUiFactory.CreateText("InstructionText", ContentRoot, 18, TextAnchor.UpperLeft, new Color(0.24f, 0.24f, 0.24f));
            instructionText.text = "\u6309 1 \u5230 25 \u7684\u987a\u5e8f\u4f9d\u6b21\u70b9\u51fb\u6570\u5b57\uff0c\u8d8a\u5feb\u8d8a\u597d\u3002";
            MiniGameUiFactory.SetAnchors(instructionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            Image statusPanel = MiniGameUiFactory.CreatePanel("StatusPanel", ContentRoot, new Color(1f, 1f, 1f, 0.82f));
            MiniGameUiFactory.SetAnchors(statusPanel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -80f), new Vector2(0f, -42f));

            statusText = MiniGameUiFactory.CreateText("StatusText", statusPanel.transform, 18, TextAnchor.MiddleLeft, new Color(0.2f, 0.2f, 0.2f));
            MiniGameUiFactory.SetAnchors(statusText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

            RectTransform gridRoot = MiniGameUiFactory.CreateRect("GridRoot", ContentRoot);
            MiniGameUiFactory.SetAnchors(gridRoot, Vector2.zero, Vector2.one, new Vector2(16f, 70f), new Vector2(-16f, -92f));

            GridLayoutGroup grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(64f, 42f);
            grid.spacing = new Vector2(6f, 6f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            for (int index = 0; index < 25; index++)
            {
                Button button = MiniGameUiFactory.CreateButton($"Cell{index}", gridRoot, string.Empty, new Color(1f, 1f, 1f, 0.95f), new Color(0.2f, 0.2f, 0.2f));
                int capturedIndex = index;
                button.onClick.AddListener(() => HandleNumberClicked(capturedIndex));
                numberButtons.Add(button);
                numberLabels.Add(button.GetComponentInChildren<Text>());
                numberImages.Add(button.GetComponent<Image>());
            }

            resultText = MiniGameUiFactory.CreateText("ResultText", ContentRoot, 17, TextAnchor.MiddleCenter, new Color(0.3f, 0.3f, 0.3f));
            MiniGameUiFactory.SetAnchors(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 42f), new Vector2(-10f, 66f));

            Image footer = MiniGameUiFactory.CreatePanel("Footer", ContentRoot, new Color(1f, 1f, 1f, 0.72f));
            MiniGameUiFactory.SetAnchors(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 34f));

            backButton = CreateBackButton(footer.transform, new Vector2(0f, 0f), new Vector2(0.20f, 1f));

            bestText = MiniGameUiFactory.CreateText("BestText", footer.transform, 17, TextAnchor.MiddleLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.22f, 0f), new Vector2(0.54f, 1f), new Vector2(10f, 0f), Vector2.zero);

            actionButton = MiniGameUiFactory.CreateButton("ActionButton", footer.transform, "\u5f00\u59cb\u6311\u6218", new Color(0.95f, 0.86f, 0.72f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
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

            elapsedTime += Time.deltaTime;
            UpdateTexts();
        }

        protected override void ResetRuntimeState()
        {
            numberButtons.Clear();
            numberLabels.Clear();
            numberImages.Clear();
            instructionText = null;
            statusText = null;
            resultText = null;
            bestText = null;
            backButton = null;
            actionButton = null;
        }

        protected override void RefreshView()
        {
            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
        }

        public override void HandleWindowClosed()
        {
            if (isPlaying && !rewardApplied)
            {
                isPlaying = false;
                resultText.text = "\u672c\u5c40\u63d0\u524d\u7ed3\u675f\uff0c\u672c\u6b21\u6309\u5931\u8d25\u7ed3\u7b97\u3002";
                ApplyMiniGameResult(MiniGameKind.SchulteGrid, false, false, 0, -1f);
                rewardApplied = true;
            }

            isPlaying = false;
            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
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
            elapsedTime = 0f;
            nextNumber = 1;

            List<int> numbers = new();
            for (int value = 1; value <= 25; value++)
            {
                numbers.Add(value);
            }

            for (int index = 0; index < numberButtons.Count; index++)
            {
                int pick = Random.Range(0, numbers.Count);
                int value = numbers[pick];
                numbers.RemoveAt(pick);

                numberLabels[index].text = value.ToString();
                numberImages[index].color = new Color(1f, 1f, 1f, 0.95f);
                numberButtons[index].interactable = true;
            }

            resultText.text = "\u4ece 1 \u5f00\u59cb\u987a\u5e8f\u70b9\u51fb\u3002";
            actionButton.GetComponentInChildren<Text>().text = "\u91cd\u65b0\u6d17\u724c";
            UpdateTexts();
        }

        private void HandleNumberClicked(int index)
        {
            if (!isPlaying || index < 0 || index >= numberLabels.Count)
            {
                return;
            }

            if (!int.TryParse(numberLabels[index].text, out int value))
            {
                return;
            }

            if (value != nextNumber)
            {
                resultText.text = $"\u5f53\u524d\u5e94\u70b9\u51fb {nextNumber}\u3002";
                return;
            }

            numberButtons[index].interactable = false;
            numberImages[index].color = new Color(0.73f, 0.91f, 0.76f, 0.95f);
            nextNumber++;

            if (nextNumber > 25)
            {
                FinishGame();
                return;
            }

            resultText.text = $"\u6b63\u786e\uff0c\u7ee7\u7eed\u627e {nextNumber}\u3002";
            UpdateTexts();
        }

        private void FinishGame()
        {
            isPlaying = false;
            float adjustedTime = ApplySessionInverseTimeModifier(elapsedTime, 2, out _);
            bool brokeRecord = bestTime <= 0f || adjustedTime < bestTime;
            string timeBreakdown = FormatSessionModifierBreakdown(elapsedTime, adjustedTime, 2);
            string modifierSuffix = string.IsNullOrEmpty(SessionScoreModifierLabel) ? string.Empty : $" ({SessionScoreModifierLabel})";
            if (bestTime <= 0f || adjustedTime < bestTime)
            {
                bestTime = adjustedTime;
                SaveStoredBestFloat(bestTime);
                resultText.text = $"\u5b8c\u6210\uff0c\u7528\u65f6 {timeBreakdown}s{modifierSuffix}\uff0c\u5237\u65b0\u6700\u4f73\u6210\u7ee9\u3002";
            }
            else
            {
                resultText.text = $"\u5b8c\u6210\uff0c\u7528\u65f6 {timeBreakdown}s{modifierSuffix}\u3002";
            }

            if (!rewardApplied)
            {
                ApplyMiniGameResult(MiniGameKind.SchulteGrid, true, brokeRecord, 25, adjustedTime);
                rewardApplied = true;
            }

            UpdateTexts();
        }

        private void SetGridInteractable(bool interactable)
        {
            foreach (Button button in numberButtons)
            {
                button.interactable = interactable;
            }
        }

        private void UpdateTexts()
        {
            if (statusText == null || bestText == null)
            {
                return;
            }

            string bestDisplay = bestTime > 0f ? $"{bestTime:0.00}s" : "--";
            statusText.text = isPlaying
                ? $"\u5f53\u524d\u76ee\u6807: {nextNumber}   \u7528\u65f6: {elapsedTime:0.00}s"
                : "\u6807\u51c6 5x5 \u8212\u5c14\u7279\u65b9\u683c";
            bestText.text = $"\u6700\u4f73\u7528\u65f6: {bestDisplay}";
        }
    }
}
