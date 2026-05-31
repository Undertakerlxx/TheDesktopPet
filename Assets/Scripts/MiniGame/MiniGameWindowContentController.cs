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
            if (success)
            {
                AchievementEventRecorder.Record(GetSuccessEventType(gameKind));
            }
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
