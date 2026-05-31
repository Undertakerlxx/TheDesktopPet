using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public partial class GeometryAtAGlanceContentController
    {
        private float GetMemorizeDuration()
        {
            if (level <= 5) return InitialMemorizeTime - ((level - 1) * MemorizeTimeDecayPerLevel);
            float levelFiveDuration = InitialMemorizeTime - (4f * MemorizeTimeDecayPerLevel);
            return Mathf.Max(MinimumMemorizeTime, levelFiveDuration - ((level - 5) * 0.35f));
        }

        private void SetCellsInteractable(bool interactable)
        {
            for (int index = 0; index < cellButtons.Count; index++) cellButtons[index].interactable = interactable;
        }

        private void ApplyPattern(GeometryCellSpec[] pattern, bool revealSelections, bool showChangeHints)
        {
            for (int index = 0; index < cellButtons.Count; index++)
            {
                bool active = index < pattern.Length;
                cellButtons[index].gameObject.SetActive(active);
                if (!active) continue;
                GeometryCellSpec cell = pattern[index];
                cellImages[index].color = cell.Color;
                cellLabels[index].text = revealSelections ? cell.Symbol : index < pattern.Length ? cell.Symbol : string.Empty;
                MiniGameUiFactory.StyleSymbolText(cellLabels[index], gridSize == 3 ? 36 : 30, new Color(0.10f, 0.10f, 0.10f), new Color(1f, 1f, 1f, 0.55f));
                if (showChangeHints)
                {
                    if (changedIndices.Contains(index)) cellImages[index].color = Color.Lerp(cell.Color, new Color(0.58f, 0.91f, 0.64f), 0.35f);
                    else if (selectedIndices.Contains(index)) cellImages[index].color = Color.Lerp(cell.Color, new Color(0.96f, 0.54f, 0.54f), 0.35f);
                }
                UpdateSelectionState(index);
            }
        }

        private void UpdateSelectionState(int index)
        {
            Outline outline = cellButtons[index].GetComponent<Outline>() ?? cellButtons[index].gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(3f, -3f);
            outline.effectColor = selectedIndices.Contains(index) ? new Color(0.13f, 0.25f, 0.48f, 0.95f) : new Color(0f, 0f, 0f, 0f);
        }

        private void EnsureGridCellCount(int totalCells)
        {
            if (cellButtons.Count < totalCells) CreateGridCells(grid.transform as RectTransform, totalCells - cellButtons.Count);
            for (int index = 0; index < cellButtons.Count; index++) cellButtons[index].gameObject.SetActive(index < totalCells);
        }

        private void CreateGridCells(RectTransform parent, int count)
        {
            for (int index = 0; index < count; index++)
            {
                int capturedIndex = cellButtons.Count;
                Button button = MiniGameUiFactory.CreateButton($"Cell{capturedIndex}", parent, "", new Color(1f, 1f, 1f, 0.95f), new Color(0.2f, 0.2f, 0.2f));
                button.onClick.AddListener(() => ToggleSelection(capturedIndex));
                cellButtons.Add(button);
                cellImages.Add(button.GetComponent<Image>());
                Text label = button.GetComponentInChildren<Text>();
                MiniGameUiFactory.StyleSymbolText(label, 36, new Color(0.10f, 0.10f, 0.10f), new Color(1f, 1f, 1f, 0.55f));
                cellLabels.Add(label);
            }
        }

        private GeometryCellSpec RandomCell()
        {
            GeometryShapeSpec shape = shapes[Random.Range(0, shapes.Length)];
            return new GeometryCellSpec(shape.Symbol, palette[Random.Range(0, palette.Length)]);
        }

        private GeometryCellSpec RandomDifferentCell(GeometryCellSpec current)
        {
            GeometryCellSpec next;
            do { next = RandomCell(); }
            while (next.Symbol == current.Symbol && Approximately(next.Color, current.Color));
            return next;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.001f &&
                   Mathf.Abs(a.g - b.g) < 0.001f &&
                   Mathf.Abs(a.b - b.b) < 0.001f &&
                   Mathf.Abs(a.a - b.a) < 0.001f;
        }

        private void ApplySessionRewards()
        {
            if (rewardApplied) return;
            int adjustedScore = ApplySessionScoreModifier(GetSessionScore(), out _);
            string scoreBreakdown = FormatSessionModifierBreakdown(GetSessionScore(), adjustedScore);
            string modifierSuffix = string.IsNullOrEmpty(SessionScoreModifierLabel) ? string.Empty : $" ({SessionScoreModifierLabel})";
            resultText.text = $"{resultText.text} \u7ed3\u7b97\u5f97\u5206 {scoreBreakdown}{modifierSuffix}\u3002";
            ApplyMiniGameResult(MiniGameKind.GeometryAtAGlance, adjustedScore >= 500, sessionBrokeRecord, adjustedScore);
            rewardApplied = true;
        }

        private int GetSessionScore()
        {
            return completedRoundsThisSession * ScorePerRound;
        }

    }

    internal readonly struct GeometryShapeSpec
    {
        public GeometryShapeSpec(string symbol, string label) { Symbol = symbol; Label = label; }
        public string Symbol { get; }
        public string Label { get; }
    }

    internal readonly struct GeometryCellSpec
    {
        public GeometryCellSpec(string symbol, Color color) { Symbol = symbol; Color = color; }
        public string Symbol { get; }
        public Color Color { get; }
    }

    internal enum GeometryPhase { Idle, Memorize, Answer, ResultWin, ResultLose }
}
