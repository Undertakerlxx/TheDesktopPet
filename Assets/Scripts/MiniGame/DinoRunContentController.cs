using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public class DinoRunContentController : MiniGameWindowContentController
    {
        private const string BestDistanceKey = "MiniGame.DinoRun.BestDistance";
        private const float SuccessDistanceThreshold = 1000f;
        private const float DinoWidth = 46f;
        private const float DinoHeight = 34f;

        private readonly List<ObstacleView> obstacles = new();

        private Text instructionText;
        private Text statusText;
        private Text resultText;
        private Text bestText;
        private Button backButton;
        private Button actionButton;
        private Button jumpButton;

        private RectTransform playfield;
        private RectTransform dinoRect;
        private Image dinoBodyImage;

        private float playfieldWidth;
        private float groundY = 8f;
        private float velocityY;
        private float jumpForce = 300f;
        private float gravity = 900f;
        private float obstacleSpeed;
        private float spawnTimer;
        private float distance;
        private float bestDistance;
        private bool isPlaying;
        private bool isGrounded = true;
        private bool rewardApplied;

        protected override void ConfigureHostWindow()
        {
            base.ConfigureHostWindow();
            SetWindowSize(StandardWindowWidth, StandardWindowHeight);
        }

        protected override void BuildContent()
        {
            bestDistance = PlayerPrefs.GetFloat(BestDistanceKey, 0f);
            rewardApplied = false;

            instructionText = MiniGameUiFactory.CreateText("InstructionText", ContentRoot, 18, TextAnchor.UpperLeft, new Color(0.24f, 0.24f, 0.24f));
            instructionText.text = "\u6309\u7a7a\u683c\u3001\u4e0a\u65b9\u5411\u952e\u6216\u70b9\u51fb\u8df3\u8dc3\uff0c\u8e72\u5f00\u969c\u788d\u7269\u3002";
            MiniGameUiFactory.SetAnchors(instructionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            Image statusPanel = MiniGameUiFactory.CreatePanel("StatusPanel", ContentRoot, new Color(1f, 1f, 1f, 0.82f));
            MiniGameUiFactory.SetAnchors(statusPanel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -80f), new Vector2(0f, -42f));

            statusText = MiniGameUiFactory.CreateText("StatusText", statusPanel.transform, 18, TextAnchor.MiddleLeft, new Color(0.2f, 0.2f, 0.2f));
            MiniGameUiFactory.SetAnchors(statusText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

            Image playfieldPanel = MiniGameUiFactory.CreatePanel("Playfield", ContentRoot, new Color(0.93f, 0.96f, 0.98f, 0.95f));
            playfield = playfieldPanel.rectTransform;
            MiniGameUiFactory.SetAnchors(playfield, Vector2.zero, Vector2.one, new Vector2(16f, 70f), new Vector2(-16f, -92f));

            Image ground = MiniGameUiFactory.CreatePanel("Ground", playfield, new Color(0.52f, 0.48f, 0.42f, 0.95f));
            MiniGameUiFactory.SetAnchors(ground.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 6f));

            dinoRect = MiniGameUiFactory.CreateRect("Dino", playfield);
            MiniGameUiFactory.SetAnchors(dinoRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, groundY), new Vector2(24f + DinoWidth, groundY + DinoHeight));
            BuildRunnerModel();

            resultText = MiniGameUiFactory.CreateText("ResultText", ContentRoot, 17, TextAnchor.MiddleCenter, new Color(0.3f, 0.3f, 0.3f));
            MiniGameUiFactory.SetAnchors(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(10f, 42f), new Vector2(-10f, 66f));

            Image footer = MiniGameUiFactory.CreatePanel("Footer", ContentRoot, new Color(1f, 1f, 1f, 0.72f));
            MiniGameUiFactory.SetAnchors(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 34f));

            backButton = CreateBackButton(footer.transform, new Vector2(0f, 0f), new Vector2(0.18f, 1f));

            bestText = MiniGameUiFactory.CreateText("BestText", footer.transform, 17, TextAnchor.MiddleLeft, new Color(0.25f, 0.25f, 0.25f));
            MiniGameUiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.20f, 0f), new Vector2(0.36f, 1f), new Vector2(10f, 0f), Vector2.zero);

            jumpButton = MiniGameUiFactory.CreateButton("JumpButton", footer.transform, "\u8df3\u8dc3", new Color(0.86f, 0.92f, 0.99f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(jumpButton.GetComponent<RectTransform>(), new Vector2(0.40f, 0f), new Vector2(0.62f, 1f), Vector2.zero, new Vector2(0f, -4f));
            jumpButton.onClick.AddListener(AttemptJump);

            actionButton = MiniGameUiFactory.CreateButton("ActionButton", footer.transform, "\u5f00\u59cb\u5954\u8dd1", new Color(0.95f, 0.86f, 0.72f, 0.95f), new Color(0.18f, 0.18f, 0.18f));
            MiniGameUiFactory.SetAnchors(actionButton.GetComponent<RectTransform>(), new Vector2(0.66f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-10f, -4f));
            actionButton.onClick.AddListener(StartGame);

            UpdateTexts();
            RefreshMiniGameAvailability(resultText, actionButton);
        }

        private void Update()
        {
            if (!isPlaying || playfield == null || dinoRect == null)
            {
                return;
            }

            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
            {
                AttemptJump();
            }

            float dt = Time.deltaTime;
            distance += obstacleSpeed * dt * 0.1f;
            obstacleSpeed += dt * 8f;
            spawnTimer -= dt;

            if (spawnTimer <= 0f)
            {
                SpawnObstacle();
            }

            UpdateDino(dt);
            UpdateObstacles(dt);
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
            jumpButton = null;
            playfield = null;
            dinoRect = null;
            dinoBodyImage = null;
            obstacles.Clear();
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
            ClearObstacles();
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

            ClearObstacles();
            isPlaying = true;
            rewardApplied = false;
            obstacleSpeed = 165f;
            spawnTimer = 1.15f;
            distance = 0f;
            velocityY = 0f;
            isGrounded = true;
            SetDinoHeight(groundY);
            resultText.text = "\u4fdd\u6301\u8282\u594f\uff0c\u969c\u788d\u4f1a\u8d8a\u6765\u8d8a\u5feb\u3002";
            actionButton.GetComponentInChildren<Text>().text = "\u91cd\u65b0\u5f00\u59cb";
            UpdateTexts();
        }

        private void AttemptJump()
        {
            if (!isPlaying || !isGrounded)
            {
                return;
            }

            velocityY = jumpForce;
            isGrounded = false;
        }

        private void UpdateDino(float dt)
        {
            if (isGrounded)
            {
                return;
            }

            velocityY -= gravity * dt;
            float nextY = dinoRect.offsetMin.y + velocityY * dt;
            if (nextY <= groundY)
            {
                nextY = groundY;
                velocityY = 0f;
                isGrounded = true;
            }

            SetDinoHeight(nextY);
        }

        private void UpdateObstacles(float dt)
        {
            for (int index = obstacles.Count - 1; index >= 0; index--)
            {
                ObstacleView obstacle = obstacles[index];
                RectTransform rect = obstacle.Rect;
                rect.anchoredPosition += Vector2.left * obstacleSpeed * dt;

                if (rect.anchoredPosition.x < -40f)
                {
                    Object.Destroy(rect.gameObject);
                    obstacles.RemoveAt(index);
                    continue;
                }

                if (IsColliding(rect))
                {
                    FinishGame();
                    return;
                }
            }
        }

        private void SpawnObstacle()
        {
            playfieldWidth = playfield.rect.width;
            bool isTallTree = Random.value > 0.45f;
            Vector2 hitboxSize = isTallTree
                ? new Vector2(Random.Range(11f, 14f), Random.Range(22f, 30f))
                : new Vector2(Random.Range(16f, 20f), Random.Range(18f, 24f));

            RectTransform obstacleRect = MiniGameUiFactory.CreateRect("Obstacle", playfield);
            obstacleRect.sizeDelta = hitboxSize;
            obstacleRect.anchorMin = new Vector2(0f, 0f);
            obstacleRect.anchorMax = new Vector2(0f, 0f);
            obstacleRect.pivot = new Vector2(0.5f, 0f);
            obstacleRect.anchoredPosition = new Vector2(playfieldWidth - 8f, 6f);
            BuildObstacle(obstacleRect, isTallTree);

            obstacles.Add(new ObstacleView(obstacleRect));
            spawnTimer = Random.Range(0.82f, 1.18f);
        }

        private bool IsColliding(RectTransform obstacle)
        {
            Vector3[] dinoCorners = new Vector3[4];
            Vector3[] obstacleCorners = new Vector3[4];
            dinoRect.GetWorldCorners(dinoCorners);
            obstacle.GetWorldCorners(obstacleCorners);

            return dinoCorners[0].x < obstacleCorners[2].x &&
                   dinoCorners[2].x > obstacleCorners[0].x &&
                   dinoCorners[0].y < obstacleCorners[2].y &&
                   dinoCorners[2].y > obstacleCorners[0].y;
        }

        private void FinishGame()
        {
            isPlaying = false;
            int rawDistance = Mathf.RoundToInt(distance);
            int adjustedDistance = ApplySessionScoreModifier(rawDistance, out _);
            bool brokeRecord = adjustedDistance > bestDistance;
            string distanceBreakdown = FormatSessionModifierBreakdown(rawDistance, adjustedDistance);
            string modifierSuffix = string.IsNullOrEmpty(SessionScoreModifierLabel) ? string.Empty : $" ({SessionScoreModifierLabel})";
            if (adjustedDistance > bestDistance)
            {
                bestDistance = adjustedDistance;
                PlayerPrefs.SetFloat(BestDistanceKey, bestDistance);
                PlayerPrefs.Save();
                resultText.text = $"\u649e\u4e0a\u969c\u788d\uff0c\u91cc\u7a0b {distanceBreakdown}{modifierSuffix}\uff0c\u5237\u65b0\u4e86\u6700\u4f73\u8bb0\u5f55\u3002";
            }
            else
            {
                resultText.text = $"\u649e\u4e0a\u969c\u788d\uff0c\u91cc\u7a0b {distanceBreakdown}{modifierSuffix}\u3002";
            }

            if (!rewardApplied)
            {
                ApplyMiniGameResult(MiniGameKind.DinoRun, adjustedDistance >= SuccessDistanceThreshold, brokeRecord, adjustedDistance);
                rewardApplied = true;
            }

            UpdateTexts();
        }

        private void ClearObstacles()
        {
            foreach (ObstacleView obstacle in obstacles)
            {
                if (obstacle.Rect != null)
                {
                    Object.Destroy(obstacle.Rect.gameObject);
                }
            }

            obstacles.Clear();
        }

        private void SetDinoHeight(float y)
        {
            dinoRect.offsetMin = new Vector2(24f, y);
            dinoRect.offsetMax = new Vector2(24f + DinoWidth, y + DinoHeight);
        }

        private void BuildRunnerModel()
        {
            dinoBodyImage = MiniGameUiFactory.CreatePanel("Body", dinoRect, new Color(0.76f, 0.80f, 0.86f, 0.98f));
            MiniGameUiFactory.SetAnchors(dinoBodyImage.rectTransform, new Vector2(0.22f, 0.20f), new Vector2(0.62f, 0.68f), Vector2.zero, Vector2.zero);

            Image head = MiniGameUiFactory.CreatePanel("Head", dinoRect, new Color(0.84f, 0.87f, 0.92f, 0.98f));
            MiniGameUiFactory.SetAnchors(head.rectTransform, new Vector2(0.54f, 0.34f), new Vector2(0.80f, 0.78f), Vector2.zero, Vector2.zero);

            Image earA = MiniGameUiFactory.CreatePanel("EarA", dinoRect, new Color(0.68f, 0.72f, 0.78f, 0.98f));
            MiniGameUiFactory.SetAnchors(earA.rectTransform, new Vector2(0.66f, 0.70f), new Vector2(0.75f, 0.90f), Vector2.zero, Vector2.zero);

            Image earB = MiniGameUiFactory.CreatePanel("EarB", dinoRect, new Color(0.68f, 0.72f, 0.78f, 0.98f));
            MiniGameUiFactory.SetAnchors(earB.rectTransform, new Vector2(0.77f, 0.70f), new Vector2(0.86f, 0.90f), Vector2.zero, Vector2.zero);

            Image tail = MiniGameUiFactory.CreatePanel("Tail", dinoRect, new Color(0.70f, 0.74f, 0.80f, 0.98f));
            MiniGameUiFactory.SetAnchors(tail.rectTransform, new Vector2(0.08f, 0.34f), new Vector2(0.18f, 0.50f), Vector2.zero, Vector2.zero);

            CreateLeg(new Vector2(0.24f, 0.00f), new Vector2(0.31f, 0.24f));
            CreateLeg(new Vector2(0.40f, 0.00f), new Vector2(0.47f, 0.24f));
            CreateLeg(new Vector2(0.56f, 0.00f), new Vector2(0.63f, 0.24f));

            Image eye = MiniGameUiFactory.CreatePanel("Eye", dinoRect, new Color(0.16f, 0.16f, 0.16f, 0.98f));
            MiniGameUiFactory.SetAnchors(eye.rectTransform, new Vector2(0.68f, 0.54f), new Vector2(0.73f, 0.60f), Vector2.zero, Vector2.zero);
        }

        private void CreateLeg(Vector2 anchorMin, Vector2 anchorMax)
        {
            Image leg = MiniGameUiFactory.CreatePanel("Leg", dinoRect, new Color(0.50f, 0.54f, 0.60f, 0.98f));
            MiniGameUiFactory.SetAnchors(leg.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }

        private void BuildObstacle(RectTransform obstacleRect, bool isTallTree)
        {
            Image trunk = MiniGameUiFactory.CreatePanel("Trunk", obstacleRect, new Color(0.47f, 0.31f, 0.19f, 0.98f));
            if (isTallTree)
            {
                MiniGameUiFactory.SetAnchors(trunk.rectTransform, new Vector2(0.22f, 0f), new Vector2(0.78f, 1f), Vector2.zero, Vector2.zero);
            }
            else
            {
                MiniGameUiFactory.SetAnchors(trunk.rectTransform, new Vector2(0.18f, 0f), new Vector2(0.82f, 1f), Vector2.zero, Vector2.zero);
            }
        }

        private void UpdateTexts()
        {
            if (statusText == null || bestText == null)
            {
                return;
            }

            int adjustedDistance = ApplySessionScoreModifier(Mathf.RoundToInt(distance), out _);
            statusText.text = isPlaying
                ? $"\u91cc\u7a0b: {adjustedDistance}   \u901f\u5ea6: {obstacleSpeed:0}"
                : "\u7ecf\u5178\u8df3\u8dc3\u8e72\u969c\u788d\u73a9\u6cd5";
            bestText.text = $"\u6700\u4f73\u91cc\u7a0b: {bestDistance:0}";
        }

        private readonly struct ObstacleView
        {
            public ObstacleView(RectTransform rect)
            {
                Rect = rect;
            }

            public RectTransform Rect { get; }
        }
    }
}
