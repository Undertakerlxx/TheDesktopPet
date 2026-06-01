using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public class DodgeBallContentController : MiniGameWindowContentController
    {
        private const float SuccessSurvivalThreshold = 20f;
        protected override MiniGameKind ControlledGameKind => MiniGameKind.DodgeBall;

        private readonly List<BallView> balls = new();

        private Text instructionText;
        private Text statusText;
        private Text resultText;
        private Text bestText;
        private Button backButton;
        private Button actionButton;
        private Button leftButton;
        private Button rightButton;

        private RectTransform playfield;
        private RectTransform playerRect;
        private float bestSurvival;
        private float survivalTime;
        private float spawnTimer;
        private float moveInput;
        private int lives;
        private bool isPlaying;
        private bool rewardApplied;

        protected override void ConfigureHostWindow()
        {
            base.ConfigureHostWindow();
            SetWindowSize(StandardWindowWidth, StandardWindowHeight);
        }

        protected override void BuildContent()
        {
            bestSurvival = LoadStoredBestFloat();
            rewardApplied = false;

            instructionText = MiniGameUiFactory.CreateText("InstructionText", ContentRoot, 18, TextAnchor.UpperLeft, new Color(0.24f, 0.24f, 0.24f));
            instructionText.text = "\u5de6\u53f3\u79fb\u52a8\u8e72\u5f00\u98de\u7403\uff0c\u88ab\u51fb\u4e2d\u4e09\u6b21\u5c31\u7ed3\u675f\u3002";
            MiniGameUiFactory.SetAnchors(instructionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            Image statusPanel = MiniGameUiFactory.CreatePanel("StatusPanel", ContentRoot, new Color(1f, 1f, 1f, 0.82f));
            MiniGameUiFactory.SetAnchors(statusPanel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -80f), new Vector2(0f, -42f));

            statusText = MiniGameUiFactory.CreateText("StatusText", statusPanel.transform, 18, TextAnchor.MiddleLeft, new Color(0.2f, 0.2f, 0.2f));
            MiniGameUiFactory.SetAnchors(statusText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

            Image playfieldPanel = MiniGameUiFactory.CreatePanel("Playfield", ContentRoot, new Color(0.93f, 0.96f, 0.98f, 0.95f));
            playfield = playfieldPanel.rectTransform;
            MiniGameUiFactory.SetAnchors(playfield, Vector2.zero, Vector2.one, new Vector2(16f, 70f), new Vector2(-16f, -92f));

            playerRect = MiniGameUiFactory.CreateRect("Player", playfield);
            playerRect.anchorMin = new Vector2(0f, 0f);
            playerRect.anchorMax = new Vector2(0f, 0f);
            playerRect.pivot = new Vector2(0.5f, 0.5f);
            playerRect.sizeDelta = new Vector2(54f, 28f);
            playerRect.anchoredPosition = new Vector2(120f, 22f);
            BuildDodgerModel();

            resultText = MiniGameUiFactory.CreateText("ResultText", ContentRoot, 17, TextAnchor.MiddleCenter, new Color(0.3f, 0.3f, 0.3f));
            MiniGameUiFactory.SetAnchors(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 42f), new Vector2(-10f, 66f));

            Image footer = MiniGameUiFactory.CreatePanel("Footer", ContentRoot, new Color(1f, 1f, 1f, 0.72f));
            MiniGameUiFactory.SetAnchors(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 34f));

            backButton = CreateBackButton(footer.transform, new Vector2(0f, 0f), new Vector2(0.16f, 1f));

            bestText = MiniGameUiFactory.CreateText("BestText", footer.transform, 17, TextAnchor.MiddleLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.18f, 0f), new Vector2(0.32f, 1f), new Vector2(10f, 0f), Vector2.zero);

            leftButton = MiniGameUiFactory.CreateButton("LeftButton", footer.transform, "\u5de6\u79fb", new Color(0.86f, 0.92f, 0.99f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(leftButton.GetComponent<RectTransform>(), new Vector2(0.36f, 0f), new Vector2(0.50f, 1f), Vector2.zero, new Vector2(0f, -4f));
            leftButton.onClick.AddListener(() => NudgePlayer(-1f));

            rightButton = MiniGameUiFactory.CreateButton("RightButton", footer.transform, "\u53f3\u79fb", new Color(0.86f, 0.92f, 0.99f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(rightButton.GetComponent<RectTransform>(), new Vector2(0.54f, 0f), new Vector2(0.68f, 1f), Vector2.zero, new Vector2(0f, -4f));
            rightButton.onClick.AddListener(() => NudgePlayer(1f));

            actionButton = MiniGameUiFactory.CreateButton("ActionButton", footer.transform, "\u5f00\u59cb\u95ea\u907f", new Color(0.95f, 0.86f, 0.72f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(actionButton.GetComponent<RectTransform>(), new Vector2(0.72f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-10f, -4f));
            actionButton.onClick.AddListener(StartGame);

            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
        }

        private void Update()
        {
            if (!isPlaying || playfield == null || playerRect == null)
            {
                return;
            }

            ReadKeyboardInput();

            float dt = Time.deltaTime;
            survivalTime += dt;
            spawnTimer -= dt;

            if (spawnTimer <= 0f)
            {
                SpawnBall();
            }

            MovePlayer(dt);
            UpdateBalls(dt);
            moveInput = 0f;
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
            leftButton = null;
            rightButton = null;
            playfield = null;
            playerRect = null;
            balls.Clear();
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
                FinishGame();
            }

            isPlaying = false;
            moveInput = 0f;
            ClearBalls();
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

            ClearBalls();
            lives = 3;
            survivalTime = 0f;
            spawnTimer = 0.8f;
            moveInput = 0f;
            isPlaying = true;
            rewardApplied = false;
            playerRect.anchoredPosition = new Vector2(playfield.rect.width * 0.5f, 22f);
            resultText.text = "\u5f00\u59cb\u95ea\u907f\uff0c\u522b\u88ab\u7403\u51fb\u4e2d\u3002";
            actionButton.GetComponentInChildren<Text>().text = "\u91cd\u65b0\u5f00\u59cb";
            UpdateTexts();
        }

        private void ReadKeyboardInput()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                moveInput = -1f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                moveInput = 1f;
            }
        }

        private void MovePlayer(float dt)
        {
            float nextX = playerRect.anchoredPosition.x + moveInput * 180f * dt;
            SetPlayerPosition(nextX);
        }

        private void NudgePlayer(float direction)
        {
            if (!isPlaying || playerRect == null)
            {
                return;
            }

            float nextX = playerRect.anchoredPosition.x + direction * 42f;
            SetPlayerPosition(nextX);
        }

        private void UpdateBalls(float dt)
        {
            for (int index = balls.Count - 1; index >= 0; index--)
            {
                BallView ball = balls[index];
                RectTransform rect = ball.Rect;
                rect.anchoredPosition += ball.Velocity * dt;

                if (rect.anchoredPosition.y < -24f)
                {
                    Object.Destroy(rect.gameObject);
                    balls.RemoveAt(index);
                    continue;
                }

                if (IsHit(rect))
                {
                    Object.Destroy(rect.gameObject);
                    balls.RemoveAt(index);
                    lives--;
                    resultText.text = $"\u88ab\u51fb\u4e2d\u4e00\u6b21\uff0c\u8fd8\u5269 {lives} \u6b21\u673a\u4f1a\u3002";
                    if (lives <= 0)
                    {
                        FinishGame();
                        return;
                    }
                }
            }
        }

        private void SpawnBall()
        {
            RectTransform ballRect = MiniGameUiFactory.CreateRect("Ball", playfield);
            ballRect.anchorMin = new Vector2(0f, 0f);
            ballRect.anchorMax = new Vector2(0f, 0f);
            ballRect.pivot = new Vector2(0.5f, 0.5f);
            ballRect.sizeDelta = new Vector2(18f, 18f);
            ballRect.anchoredPosition = new Vector2(Random.Range(18f, playfield.rect.width - 18f), playfield.rect.height - 14f);

            Image image = ballRect.gameObject.AddComponent<Image>();
            image.color = new Color(0.9f, 0.42f, 0.38f, 0.98f);

            Vector2 velocity = new(Random.Range(-45f, 45f), -Random.Range(140f, 190f) - survivalTime * 3f);
            balls.Add(new BallView(ballRect, velocity));
            spawnTimer = Mathf.Max(0.28f, 0.8f - survivalTime * 0.02f);
        }

        private bool IsHit(RectTransform ball)
        {
            Vector2 playerPos = playerRect.anchoredPosition;
            Vector2 ballPos = ball.anchoredPosition;
            Vector2 playerHalf = playerRect.sizeDelta * 0.5f;
            Vector2 ballHalf = ball.sizeDelta * 0.5f;

            return Mathf.Abs(playerPos.x - ballPos.x) < playerHalf.x + ballHalf.x &&
                   Mathf.Abs(playerPos.y - ballPos.y) < playerHalf.y + ballHalf.y;
        }

        private void FinishGame()
        {
            isPlaying = false;
            float adjustedSurvival = ApplySessionPositiveTimeModifier(survivalTime, 1, out _);
            bool brokeRecord = adjustedSurvival > bestSurvival;
            string survivalBreakdown = FormatSessionModifierBreakdown(survivalTime, adjustedSurvival, 1);
            string modifierSuffix = string.IsNullOrEmpty(SessionScoreModifierLabel) ? string.Empty : $" ({SessionScoreModifierLabel})";
            if (adjustedSurvival > bestSurvival)
            {
                bestSurvival = adjustedSurvival;
                SaveStoredBestFloat(bestSurvival);
                resultText.text = $"\u6311\u6218\u7ed3\u675f\uff0c\u575a\u6301\u4e86 {survivalBreakdown}s{modifierSuffix}\uff0c\u5237\u65b0\u6700\u4f73\u8bb0\u5f55\u3002";
            }
            else
            {
                resultText.text = $"\u6311\u6218\u7ed3\u675f\uff0c\u575a\u6301\u4e86 {survivalBreakdown}s{modifierSuffix}\u3002";
            }

            if (!rewardApplied)
            {
                ApplyMiniGameResult(MiniGameKind.DodgeBall, adjustedSurvival >= SuccessSurvivalThreshold, brokeRecord, Mathf.RoundToInt(adjustedSurvival * 10f));
                rewardApplied = true;
            }

            UpdateTexts();
        }

        private void ClearBalls()
        {
            foreach (BallView ball in balls)
            {
                if (ball.Rect != null)
                {
                    Object.Destroy(ball.Rect.gameObject);
                }
            }

            balls.Clear();
        }

        private void SetPlayerPosition(float x)
        {
            float halfWidth = playerRect.sizeDelta.x * 0.5f;
            float clampedX = Mathf.Clamp(x, halfWidth, playfield.rect.width - halfWidth);
            playerRect.anchoredPosition = new Vector2(clampedX, playerRect.anchoredPosition.y);
        }

        private void BuildDodgerModel()
        {
            Image body = MiniGameUiFactory.CreatePanel("Body", playerRect, new Color(0.84f, 0.86f, 0.90f, 0.98f));
            MiniGameUiFactory.SetAnchors(body.rectTransform, new Vector2(0.18f, 0.20f), new Vector2(0.82f, 0.72f), Vector2.zero, Vector2.zero);

            Image head = MiniGameUiFactory.CreatePanel("Head", playerRect, new Color(0.91f, 0.93f, 0.97f, 0.98f));
            MiniGameUiFactory.SetAnchors(head.rectTransform, new Vector2(0.58f, 0.44f), new Vector2(0.92f, 0.88f), Vector2.zero, Vector2.zero);

            Image earA = MiniGameUiFactory.CreatePanel("EarA", playerRect, new Color(0.74f, 0.78f, 0.84f, 0.98f));
            MiniGameUiFactory.SetAnchors(earA.rectTransform, new Vector2(0.62f, 0.78f), new Vector2(0.72f, 0.98f), Vector2.zero, Vector2.zero);

            Image earB = MiniGameUiFactory.CreatePanel("EarB", playerRect, new Color(0.74f, 0.78f, 0.84f, 0.98f));
            MiniGameUiFactory.SetAnchors(earB.rectTransform, new Vector2(0.80f, 0.78f), new Vector2(0.90f, 0.98f), Vector2.zero, Vector2.zero);

            CreatePaw(new Vector2(0.26f, 0.00f), new Vector2(0.34f, 0.20f));
            CreatePaw(new Vector2(0.46f, 0.00f), new Vector2(0.54f, 0.20f));
            CreatePaw(new Vector2(0.64f, 0.00f), new Vector2(0.72f, 0.20f));

            Image eye = MiniGameUiFactory.CreatePanel("Eye", playerRect, new Color(0.18f, 0.18f, 0.18f, 0.98f));
            MiniGameUiFactory.SetAnchors(eye.rectTransform, new Vector2(0.76f, 0.60f), new Vector2(0.80f, 0.68f), Vector2.zero, Vector2.zero);
        }

        private void CreatePaw(Vector2 anchorMin, Vector2 anchorMax)
        {
            Image paw = MiniGameUiFactory.CreatePanel("Paw", playerRect, new Color(0.58f, 0.62f, 0.68f, 0.98f));
            MiniGameUiFactory.SetAnchors(paw.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }

        private void UpdateTexts()
        {
            if (statusText == null || bestText == null)
            {
                return;
            }

            float adjustedSurvival = ApplySessionPositiveTimeModifier(survivalTime, 1, out _);
            statusText.text = isPlaying
                ? $"\u751f\u5b58: {adjustedSurvival:0.0}s   \u751f\u547d: {lives}"
                : "\u5de6\u53f3\u79fb\u52a8\u8e72\u907f\u6765\u7403";
            bestText.text = $"\u6700\u4f73\u751f\u5b58: {bestSurvival:0.0}s";
        }

        private readonly struct BallView
        {
            public BallView(RectTransform rect, Vector2 velocity)
            {
                Rect = rect;
                Velocity = velocity;
            }

            public RectTransform Rect { get; }
            public Vector2 Velocity { get; }
        }
    }
}
