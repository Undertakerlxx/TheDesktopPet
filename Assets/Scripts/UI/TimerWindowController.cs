using System.Collections.Generic;
using DesktopPet.Storage;
using DesktopPet.Timer;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class TimerWindowController : UIWindowController
    {
        public GameObject mainPage;
        public GameObject estimatePage;
        public GameObject historyPage;

        public Text timeText;
        public InputField taskInput;
        public Button startButton;
        public Text startButtonText;
        public Button estimateButton;
        public Button historyButton;
        public Button closeButton;

        public InputField estimateHourInput;
        public InputField estimateMinuteInput;
        public InputField estimateSecondInput;
        public Button estimateConfirmButton;
        public Button estimateCancelButton;

        public ScrollRect historyScrollRect;
        public RectTransform historyContent;
        public Button historyBackButton;

        public Color normalTimeColor = new Color(0.18f, 0.16f, 0.14f, 1f);
        public Color overtimeColor = new Color(0.86f, 0.15f, 0.12f, 1f);

        private readonly WorkTimer timer = new WorkTimer();
        private readonly List<TimerHistoryRecord> records = new List<TimerHistoryRecord>();
        private TimerHistoryStorage historyStorage;
        private bool suppressEstimateInputEvents;

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            historyStorage = new TimerHistoryStorage();
            records.Clear();
            records.AddRange(historyStorage.Load());
            BindButtons();
            BindEstimateInputConstraints();
            ShowMainPage();
            RefreshTimeText();
        }

        public override void Open()
        {
            base.Open();
            ShowMainPage();
            RefreshTimeText();
        }

        private void Update()
        {
            if (!IsOpen())
            {
                return;
            }

            timer.Tick();
            RefreshTimeText();
        }

        private void BindButtons()
        {
            BindButton(startButton, ToggleTimer);
            BindButton(estimateButton, ShowEstimatePage);
            BindButton(historyButton, ShowHistoryPage);
            BindButton(estimateConfirmButton, ConfirmEstimateTime);
            BindButton(estimateCancelButton, ShowMainPage);
            BindButton(historyBackButton, ShowMainPage);
            BindButton(closeButton, () => uiManager.CloseWindow(windowType));
        }

        private void BindEstimateInputConstraints()
        {
            BindLimitedNumberInput(estimateMinuteInput, 59);
            BindLimitedNumberInput(estimateSecondInput, 59);
        }

        private void BindLimitedNumberInput(InputField inputField, int maxValue)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(value => ClampInputField(inputField, value, maxValue));
        }

        private void ClampInputField(InputField inputField, string value, int maxValue)
        {
            if (suppressEstimateInputEvents || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!int.TryParse(value, out int number))
            {
                SetInputTextSilently(inputField, string.Empty);
                return;
            }

            number = Mathf.Clamp(number, 0, maxValue);
            string clampedText = number.ToString();
            if (value != clampedText)
            {
                SetInputTextSilently(inputField, clampedText);
            }
        }

        private void SetInputTextSilently(InputField inputField, string value)
        {
            suppressEstimateInputEvents = true;
            inputField.text = value;
            suppressEstimateInputEvents = false;
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);
        }

        private void ToggleTimer()
        {
            if (timer.IsTiming)
            {
                FinishTimer();
                return;
            }

            StartTimer();
        }

        private void StartTimer()
        {
            string task = taskInput != null ? taskInput.text : string.Empty;
            timer.Start(task);

            if (startButtonText != null)
            {
                startButtonText.text = "完成";
            }

            ShowMainPage();
        }

        private void FinishTimer()
        {
            TimerHistoryRecord record = timer.Finish();
            records.Insert(0, record);
            historyStorage.Save(records);

            if (startButtonText != null)
            {
                startButtonText.text = "开始";
            }

            RefreshHistoryList();
            RefreshTimeText();
        }

        private void ConfirmEstimateTime()
        {
            int hours = ReadInputNumber(estimateHourInput, int.MaxValue);
            int minutes = ReadInputNumber(estimateMinuteInput, 59);
            int seconds = ReadInputNumber(estimateSecondInput, 59);
            timer.SetEstimate(hours, minutes, seconds);
            ShowMainPage();
        }

        private static int ReadInputNumber(InputField inputField, int maxValue)
        {
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
            {
                return 0;
            }

            if (!int.TryParse(inputField.text, out int value))
            {
                return 0;
            }

            return Mathf.Clamp(value, 0, maxValue);
        }

        private void ShowMainPage()
        {
            SetPage(mainPage, true);
            SetPage(estimatePage, false);
            SetPage(historyPage, false);
        }

        private void ShowEstimatePage()
        {
            SetPage(mainPage, false);
            SetPage(estimatePage, true);
            SetPage(historyPage, false);
        }

        private void ShowHistoryPage()
        {
            RefreshHistoryList();
            SetPage(mainPage, false);
            SetPage(estimatePage, false);
            SetPage(historyPage, true);
        }

        private static void SetPage(GameObject page, bool active)
        {
            if (page != null)
            {
                page.SetActive(active);
            }
        }

        private void RefreshTimeText()
        {
            if (timeText == null)
            {
                return;
            }

            if (!timer.IsTiming)
            {
                timeText.text = System.DateTime.Now.ToString("HH:mm:ss");
                timeText.color = normalTimeColor;
                return;
            }

            timeText.text = WorkTimer.FormatDuration(timer.ElapsedSeconds);
            timeText.color = timer.IsOvertime ? overtimeColor : normalTimeColor;
        }

        private void RefreshHistoryList()
        {
            if (historyContent == null)
            {
                return;
            }

            for (int i = historyContent.childCount - 1; i >= 0; i--)
            {
                Destroy(historyContent.GetChild(i).gameObject);
            }

            if (records.Count == 0)
            {
                CreateHistoryRow("暂无历史记录", string.Empty);
                ResetHistoryScroll();
                return;
            }

            foreach (TimerHistoryRecord record in records)
            {
                CreateHistoryRow(record.task, WorkTimer.FormatDuration(record.elapsedSeconds));
            }

            ResetHistoryScroll();
        }

        private void ResetHistoryScroll()
        {
            if (historyScrollRect != null)
            {
                historyScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void CreateHistoryRow(string task, string duration)
        {
            GameObject row = new GameObject("HistoryRow", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            row.transform.SetParent(historyContent, false);

            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 48f);

            LayoutElement layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 48f;

            Image rowImage = row.GetComponent<Image>();
            rowImage.color = new Color(1f, 1f, 1f, 0.78f);

            Text taskText = CreateRowText(row.transform, "Task", task, TextAnchor.MiddleLeft);
            RectTransform taskRect = taskText.GetComponent<RectTransform>();
            taskRect.anchorMin = new Vector2(0f, 0f);
            taskRect.anchorMax = new Vector2(0.52f, 1f);
            taskRect.offsetMin = new Vector2(12f, 0f);
            taskRect.offsetMax = new Vector2(-8f, 0f);

            Text durationText = CreateRowText(row.transform, "Duration", duration, TextAnchor.MiddleCenter);
            RectTransform durationRect = durationText.GetComponent<RectTransform>();
            durationRect.anchorMin = new Vector2(0.56f, 0f);
            durationRect.anchorMax = new Vector2(1f, 1f);
            durationRect.offsetMin = new Vector2(8f, 0f);
            durationRect.offsetMax = new Vector2(-12f, 0f);
        }

        private Text CreateRowText(Transform parent, string name, string value, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.text = value;
            text.fontSize = 18;
            text.color = normalTimeColor;
            text.alignment = alignment;
            return text;
        }

        private bool IsOpen()
        {
            return windowLayer != null && windowLayer.IsVisible;
        }
    }
}