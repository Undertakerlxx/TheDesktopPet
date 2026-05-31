using DesktopPet.Achievements;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    [RequireComponent(typeof(FeatureWindowController))]
    public abstract class MiniGameWindowContentController : MonoBehaviour
    {
        protected const float StandardWindowWidth = 540f;
        protected const float StandardWindowHeight = 540f;

        protected FeatureWindowController HostWindow { get; private set; }
        protected UIManager UIManager { get; private set; }
        protected RectTransform ContentRoot { get; private set; }
        protected int SessionScoreModifierPercent { get; private set; }
        protected string SessionScoreModifierLabel { get; private set; }

        private bool isBuilt;
        private ThePetStatsManager cachedStatsManager;

        public void InitializeContent(FeatureWindowController hostWindow, UIManager uiManager)
        {
            HostWindow = hostWindow;
            UIManager = uiManager;
            ConfigureHostWindow();

            if (ContentRoot == null)
            {
                isBuilt = false;
                ResetRuntimeState();
            }

            if (!isBuilt)
            {
                ContentRoot = EnsureContentRoot();
                BuildContent();
                isBuilt = true;
            }

            RefreshView();
        }

        public virtual void HandleWindowOpened()
        {
            RefreshView();
        }

        public virtual void HandleWindowClosed()
        {
        }

        protected abstract void BuildContent();
        protected abstract MiniGameKind ControlledGameKind { get; }

        protected virtual void RefreshView()
        {
        }

        protected virtual void ResetRuntimeState()
        {
        }

        protected virtual void ConfigureHostWindow()
        {
            SetWindowSize(StandardWindowWidth, StandardWindowHeight);

            if (HostWindow.titleText != null)
            {
                RectTransform titleRect = HostWindow.titleText.rectTransform;
                HostWindow.titleText.fontSize = 26;
                HostWindow.titleText.alignment = TextAnchor.MiddleLeft;
                HostWindow.titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
                HostWindow.titleText.verticalOverflow = VerticalWrapMode.Truncate;
                if (titleRect != null)
                {
                    titleRect.anchorMin = new Vector2(0f, 1f);
                    titleRect.anchorMax = new Vector2(1f, 1f);
                    titleRect.offsetMin = new Vector2(18f, -68f);
                    titleRect.offsetMax = new Vector2(-98f, -24f);
                }
            }

            if (HostWindow.closeButton != null)
            {
                RectTransform closeRect = HostWindow.closeButton.GetComponent<RectTransform>();
                if (closeRect != null)
                {
                    closeRect.anchorMin = new Vector2(1f, 1f);
                    closeRect.anchorMax = new Vector2(1f, 1f);
                    closeRect.pivot = new Vector2(1f, 1f);
                    closeRect.sizeDelta = new Vector2(72f, 40f);
                    closeRect.anchoredPosition = new Vector2(-18f, -24f);
                }

                Text closeLabel = HostWindow.closeButton.GetComponentInChildren<Text>();
                if (closeLabel != null)
                {
                    closeLabel.fontSize = 18;
                    closeLabel.alignment = TextAnchor.MiddleCenter;
                    closeLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                    closeLabel.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }

            if (HostWindow.bodyText != null)
            {
                HostWindow.bodyText.gameObject.SetActive(false);
            }
        }

        protected void SetWindowSize(float width, float height)
        {
            RectTransform windowRect = HostWindow.GetComponent<RectTransform>();
            if (windowRect != null)
            {
                windowRect.sizeDelta = new Vector2(width, height);
            }
        }

        protected Button CreateBackButton(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            Button button = MiniGameUiFactory.CreateButton("BackButton", parent, "\u8fd4\u56de", new Color(0.90f, 0.94f, 0.99f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(button.GetComponent<RectTransform>(), anchorMin, anchorMax, new Vector2(10f, 4f), Vector2.zero);
            button.onClick.AddListener(ReturnToHub);
            return button;
        }

        protected void ReturnToHub()
        {
            UIManager?.OpenWindow(UIWindowType.MiniGame);
        }

        protected void ApplyMiniGameResult(MiniGameKind gameKind, bool success, bool brokeRecord, int score = 0, float completionSeconds = -1f)
        {
            cachedStatsManager ??= FindFirstObjectByType<ThePetStatsManager>();
            cachedStatsManager?.ApplyMiniGameResult(gameKind, success, brokeRecord, score, completionSeconds);

            AchievementEventRecorder.Record(AchievementEventType.MiniGamePlayed);
            AchievementEventRecorder.Record($"MiniGamePlayed.{gameKind}");
            if (success)
            {
                AchievementEventRecorder.Record(GetSuccessEventType(gameKind));
            }
        }

        protected bool TryBeginMiniGameSession(Text resultText = null)
        {
            cachedStatsManager ??= FindFirstObjectByType<ThePetStatsManager>();
            if (cachedStatsManager != null && !cachedStatsManager.CanStartMiniGame(ControlledGameKind, out string reason))
            {
                SessionScoreModifierPercent = 0;
                SessionScoreModifierLabel = string.Empty;
                if (resultText != null)
                {
                    resultText.text = reason;
                }

                return false;
            }

            SessionScoreModifierPercent = cachedStatsManager != null ? cachedStatsManager.GetMiniGameScoreModifierPercent() : 0;
            SessionScoreModifierLabel = cachedStatsManager != null ? cachedStatsManager.GetMiniGameScoreModifierLabel() : string.Empty;
            return true;
        }

        protected void RefreshMiniGameAvailability(Text resultText, Button actionButton)
        {
            cachedStatsManager ??= FindFirstObjectByType<ThePetStatsManager>();
            string reason = string.Empty;
            bool canPlay = cachedStatsManager == null || cachedStatsManager.CanStartMiniGame(ControlledGameKind, out reason);
            if (actionButton != null)
            {
                actionButton.interactable = canPlay;
            }

            if (resultText != null && !canPlay)
            {
                resultText.text = reason;
            }
            else if (resultText != null && IsAvailabilityMessage(resultText.text))
            {
                resultText.text = string.Empty;
            }
        }

        protected int ApplySessionScoreModifier(int rawValue, out int delta)
        {
            delta = Mathf.RoundToInt(rawValue * Mathf.Abs(SessionScoreModifierPercent) / 100f);
            if (SessionScoreModifierPercent > 0)
            {
                return rawValue + delta;
            }

            if (SessionScoreModifierPercent < 0)
            {
                return Mathf.Max(0, rawValue - delta);
            }

            delta = 0;
            return rawValue;
        }

        protected float ApplySessionPositiveTimeModifier(float rawValue, int decimals, out float delta)
        {
            delta = RoundToDecimals(rawValue * Mathf.Abs(SessionScoreModifierPercent) / 100f, decimals);
            if (SessionScoreModifierPercent > 0)
            {
                return RoundToDecimals(rawValue + delta, decimals);
            }

            if (SessionScoreModifierPercent < 0)
            {
                return Mathf.Max(0f, RoundToDecimals(rawValue - delta, decimals));
            }

            delta = 0f;
            return RoundToDecimals(rawValue, decimals);
        }

        protected float ApplySessionInverseTimeModifier(float rawValue, int decimals, out float delta)
        {
            delta = RoundToDecimals(rawValue * Mathf.Abs(SessionScoreModifierPercent) / 100f, decimals);
            if (SessionScoreModifierPercent > 0)
            {
                return Mathf.Max(0f, RoundToDecimals(rawValue - delta, decimals));
            }

            if (SessionScoreModifierPercent < 0)
            {
                return RoundToDecimals(rawValue + delta, decimals);
            }

            delta = 0f;
            return RoundToDecimals(rawValue, decimals);
        }

        protected string FormatSessionModifierBreakdown(int rawValue, int adjustedValue)
        {
            int delta = Mathf.Abs(adjustedValue - rawValue);
            return delta <= 0 || SessionScoreModifierPercent == 0
                ? $"{adjustedValue}"
                : $"{rawValue}{(SessionScoreModifierPercent > 0 ? "+" : "-")}{delta}={adjustedValue}";
        }

        protected string FormatSessionModifierBreakdown(float rawValue, float adjustedValue, int decimals)
        {
            float delta = RoundToDecimals(Mathf.Abs(adjustedValue - rawValue), decimals);
            string rawText = rawValue.ToString($"F{decimals}");
            string deltaText = delta.ToString($"F{decimals}");
            string adjustedText = adjustedValue.ToString($"F{decimals}");
            return delta <= 0f || SessionScoreModifierPercent == 0
                ? adjustedText
                : $"{rawText}{(SessionScoreModifierPercent > 0 ? "+" : "-")}{deltaText}={adjustedText}";
        }

        private static float RoundToDecimals(float value, int decimals)
        {
            float multiplier = Mathf.Pow(10f, decimals);
            return Mathf.Round(value * multiplier) / multiplier;
        }

        private static bool IsAvailabilityMessage(string message)
        {
            return message == "\u9965\u997f\u503c\u4f4e\u4e8e30\uff0c\u65e0\u6cd5\u8fdb\u884c\u5c0f\u6e38\u620f\u3002" ||
                   message == "\u6d3b\u529b\u503c\u8fc7\u4f4e\uff0c\u65e0\u6cd5\u8fdb\u884c\u5c0f\u6e38\u620f\u3002" ||
                   (!string.IsNullOrEmpty(message) && message.StartsWith("亲密度达到 "));
        }

        private static AchievementEventType GetSuccessEventType(MiniGameKind gameKind)
        {
            return gameKind switch
            {
                MiniGameKind.SchulteGrid => AchievementEventType.FocusGameSuccess,
                MiniGameKind.ColorGrid => AchievementEventType.FocusGameSuccess,
                MiniGameKind.EyeHandSpeed => AchievementEventType.ReactionGameSuccess,
                MiniGameKind.GeometryAtAGlance => AchievementEventType.ReactionGameSuccess,
                MiniGameKind.DinoRun => AchievementEventType.MovementGameSuccess,
                MiniGameKind.DodgeBall => AchievementEventType.MovementGameSuccess,
                _ => AchievementEventType.MiniGamePlayed
            };
        }

        private RectTransform EnsureContentRoot()
        {
            Transform existing = transform.Find("RuntimeContentRoot");
            if (existing != null)
            {
                return existing as RectTransform;
            }

            GameObject rootObject = new("RuntimeContentRoot", typeof(RectTransform));
            rootObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            RectTransform rectTransform = rootObject.GetComponent<RectTransform>();
            rectTransform.SetParent(transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(18f, 16f);
            rectTransform.offsetMax = new Vector2(-18f, -74f);
            return rectTransform;
        }
    }
}
