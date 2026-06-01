using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class AIChatPanelController : MonoBehaviour
    {
        private const string RootObjectName = "AIChatPanel";
        private const float HorizontalPadding = 24f;
        private const float VerticalPadding = 20f;
        private const float HeaderHeight = 52f;
        private const float ButtonWidth = 110f;
        private const float ButtonHeight = 34f;
        private const float ScrollbarWidth = 14f;
        private const float InputAreaHeight = 116f;
        private const float StatusHeight = 42f;
        private const float BottomTipHeight = 56f;
        private const float SettingsBottomAreaHeight = 88f;
        private const float SettingsFieldHeight = 36f;
        private const float SettingsFieldGap = 86f;

        private UIManager ownerManager;
        private RectTransform rootRectTransform;
        private RectTransform panelRectTransform;
        private GameObject chatPage;
        private GameObject settingsPage;
        private ScrollRect historyScrollRect;
        private RectTransform historyContentRect;
        private Scrollbar historyScrollbar;
        private Text chatStatusText;
        private Text settingsStatusText;
        private InputField endpointInput;
        private InputField apiKeyInput;
        private InputField modelInput;
        private InputField messageInput;
        private Button sendButton;
        private Button saveButton;
        private bool isSending;

        private readonly List<AIChatMessageData> conversation = new List<AIChatMessageData>();

        public static AIChatPanelController Show(Transform parent, UIManager ownerManager)
        {
            Transform oldPanel = parent.Find(RootObjectName);
            if (oldPanel != null)
            {
                Destroy(oldPanel.gameObject);
            }

            GameObject root = new GameObject(RootObjectName, typeof(RectTransform), typeof(AIChatPanelController));
            root.transform.SetParent(parent, false);

            AIChatPanelController controller = root.GetComponent<AIChatPanelController>();
            controller.ownerManager = ownerManager;
            controller.Initialize();
            return controller;
        }

        private void Initialize()
        {
            rootRectTransform = GetComponent<RectTransform>();
            rootRectTransform.anchorMin = Vector2.zero;
            rootRectTransform.anchorMax = Vector2.one;
            rootRectTransform.offsetMin = Vector2.zero;
            rootRectTransform.offsetMax = Vector2.zero;

            BuildPanel();
            BuildChatPage();
            BuildSettingsPage();
            LoadSavedConfig();
            ShowChatPage();
            SetChatStatus("\u53ef\u4ee5\u5148\u5728\u8bbe\u7f6e\u91cc\u586b\u5199 API \u4fe1\u606f\uff0c\u7136\u540e\u5f00\u59cb\u804a\u5929\u3002", false);
            SetSettingsStatus(string.Empty, false);
        }

        private void BuildPanel()
        {
            GameObject panelObject = new GameObject("ChatPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(transform, false);

            panelRectTransform = panelObject.GetComponent<RectTransform>();
            panelRectTransform.anchorMin = Vector2.zero;
            panelRectTransform.anchorMax = Vector2.one;
            panelRectTransform.offsetMin = Vector2.zero;
            panelRectTransform.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.96f, 0.97f, 0.99f, 0.98f);
        }

        private void BuildChatPage()
        {
            chatPage = CreatePageRoot("ChatPage");
            BuildPageHeader(chatPage.transform, "AI \u804a\u5929", ShowSettingsPage, "\u8bbe\u7f6e");

            Text historyLabel = CreateText(chatPage.transform, "HistoryLabel", 13, TextAnchor.MiddleLeft, new Color(0.40f, 0.44f, 0.50f, 1f));
            SetRect(
                historyLabel.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(HorizontalPadding, -VerticalPadding - HeaderHeight - 24f),
                new Vector2(-HorizontalPadding, -VerticalPadding - HeaderHeight));
            historyLabel.text = "\u5bf9\u8bdd";

            BuildHistoryArea(chatPage.transform);

            chatStatusText = CreateText(chatPage.transform, "ChatStatusText", 11, TextAnchor.UpperLeft, new Color(0.24f, 0.31f, 0.40f, 1f));
            chatStatusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            chatStatusText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(
                chatStatusText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(HorizontalPadding, InputAreaHeight + 12f),
                new Vector2(-HorizontalPadding, InputAreaHeight + 12f + StatusHeight));

            Text inputLabel = CreateText(chatPage.transform, "InputLabel", 13, TextAnchor.MiddleLeft, new Color(0.40f, 0.44f, 0.50f, 1f));
            SetRect(
                inputLabel.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(HorizontalPadding, 84f),
                new Vector2(-HorizontalPadding, 104f));
            inputLabel.text = "\u8f93\u5165";

            messageInput = CreateInputField(chatPage.transform, "MessageInput", false, "\u60f3\u548c AI \u804a\u4ec0\u4e48\uff1f", 13, InputField.LineType.MultiLineNewline);
            SetRect(
                messageInput.GetComponent<RectTransform>(),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(HorizontalPadding, 20f),
                new Vector2(-HorizontalPadding - ButtonWidth - 16f, 76f));

            sendButton = CreateButton(chatPage.transform, "SendButton", "\u53d1\u9001", 15);
            SetRect(
                sendButton.GetComponent<RectTransform>(),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-HorizontalPadding - ButtonWidth, 20f),
                new Vector2(-HorizontalPadding, 76f));
            sendButton.onClick.AddListener(SendMessage);
        }

        private void BuildSettingsPage()
        {
            settingsPage = CreatePageRoot("SettingsPage");
            BuildPageHeader(settingsPage.transform, "AI \u804a\u5929\u8bbe\u7f6e", ShowChatPage, "\u8fd4\u56de");

            CreateSettingsField(settingsPage.transform, "URL", "EndpointInput", "https://.../chat/completions", false, 0, out endpointInput);
            CreateSettingsField(settingsPage.transform, "API Key", "ApiKeyInput", "sk-...", true, 1, out apiKeyInput);
            CreateSettingsField(settingsPage.transform, "Model", "ModelInput", "deepseek-v4-flash", false, 2, out modelInput);

            saveButton = CreateButton(settingsPage.transform, "SaveButton", "\u4fdd\u5b58", 15);
            SetRect(
                saveButton.GetComponent<RectTransform>(),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(HorizontalPadding, BottomTipHeight + 18f),
                new Vector2(HorizontalPadding + ButtonWidth, BottomTipHeight + 18f + ButtonHeight));
            saveButton.onClick.AddListener(SaveConfig);

            settingsStatusText = CreateText(settingsPage.transform, "SettingsStatusText", 11, TextAnchor.UpperLeft, new Color(0.24f, 0.31f, 0.40f, 1f));
            settingsStatusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            settingsStatusText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(
                settingsStatusText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(HorizontalPadding + ButtonWidth + 18f, BottomTipHeight + 18f),
                new Vector2(-HorizontalPadding, BottomTipHeight + 18f + SettingsBottomAreaHeight));

            Text tipText = CreateText(settingsPage.transform, "TipText", 11, TextAnchor.UpperLeft, new Color(0.40f, 0.44f, 0.50f, 1f));
            tipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            tipText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(
                tipText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(HorizontalPadding, 14f),
                new Vector2(-HorizontalPadding, BottomTipHeight));
            tipText.text = "\u5efa\u8bae\u5728 URL \u4e2d\u586b\u5199\u5b8c\u6574 chat completions \u63a5\u53e3\u5730\u5740\uff0cAPI Key \u548c Model \u4f1a\u4fdd\u5b58\u5728\u672c\u5730 PlayerPrefs \u4e2d\u3002";
        }

        private void BuildPageHeader(Transform parent, string title, UnityEngine.Events.UnityAction secondaryAction, string secondaryLabel)
        {
            Text titleText = CreateText(parent, "Title", 24, TextAnchor.MiddleLeft, new Color(0.16f, 0.18f, 0.23f, 1f));
            SetRect(
                titleText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(HorizontalPadding, -VerticalPadding - HeaderHeight),
                new Vector2(-HorizontalPadding - ButtonWidth * 2f - 20f, -VerticalPadding));
            titleText.text = title;

            Button secondaryButton = CreateButton(parent, "SecondaryButton", secondaryLabel, 14);
            SetRect(
                secondaryButton.GetComponent<RectTransform>(),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-HorizontalPadding - ButtonWidth * 2f - 10f, -VerticalPadding - ButtonHeight),
                new Vector2(-HorizontalPadding - ButtonWidth - 10f, -VerticalPadding));
            secondaryButton.onClick.AddListener(secondaryAction);

            Button closeButton = CreateButton(parent, "CloseButton", "\u5173\u95ed", 14);
            SetRect(
                closeButton.GetComponent<RectTransform>(),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-HorizontalPadding - ButtonWidth, -VerticalPadding - ButtonHeight),
                new Vector2(-HorizontalPadding, -VerticalPadding));
            closeButton.onClick.AddListener(Close);
        }

        private void BuildHistoryArea(Transform parent)
        {
            GameObject scrollObject = new GameObject("HistoryScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(parent, false);

            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            SetRect(
                scrollRectTransform,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(HorizontalPadding, InputAreaHeight + StatusHeight + 18f),
                new Vector2(-HorizontalPadding, -VerticalPadding - HeaderHeight - 30f));

            Image scrollBackground = scrollObject.GetComponent<Image>();
            scrollBackground.color = new Color(1f, 1f, 1f, 0.92f);

            historyScrollRect = scrollObject.GetComponent<ScrollRect>();
            historyScrollRect.horizontal = false;
            historyScrollRect.movementType = ScrollRect.MovementType.Clamped;
            historyScrollRect.scrollSensitivity = 25f;

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);

            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            SetRect(
                viewportRect,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 0f),
                new Vector2(-ScrollbarWidth - 6f, 0f));

            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);

            Mask viewportMask = viewportObject.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            GameObject scrollbarObject = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObject.transform.SetParent(scrollObject.transform, false);

            RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            SetRect(
                scrollbarRect,
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(-ScrollbarWidth, 0f),
                new Vector2(0f, 0f));

            Image scrollbarBackground = scrollbarObject.GetComponent<Image>();
            scrollbarBackground.color = new Color(0.89f, 0.90f, 0.93f, 1f);

            GameObject handleSlidingAreaObject = new GameObject("SlidingArea", typeof(RectTransform));
            handleSlidingAreaObject.transform.SetParent(scrollbarObject.transform, false);

            RectTransform slidingAreaRect = handleSlidingAreaObject.GetComponent<RectTransform>();
            slidingAreaRect.anchorMin = Vector2.zero;
            slidingAreaRect.anchorMax = Vector2.one;
            slidingAreaRect.offsetMin = new Vector2(2f, 2f);
            slidingAreaRect.offsetMax = new Vector2(-2f, -2f);

            GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObject.transform.SetParent(handleSlidingAreaObject.transform, false);

            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;

            Image handleImage = handleObject.GetComponent<Image>();
            handleImage.color = new Color(0.85f, 0.70f, 0.28f, 1f);

            historyScrollbar = scrollbarObject.GetComponent<Scrollbar>();
            historyScrollbar.direction = Scrollbar.Direction.BottomToTop;
            historyScrollbar.handleRect = handleRect;
            historyScrollbar.targetGraphic = handleImage;
            historyScrollbar.size = 0.2f;

            GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);

            historyContentRect = contentObject.GetComponent<RectTransform>();
            historyContentRect.anchorMin = new Vector2(0f, 1f);
            historyContentRect.anchorMax = new Vector2(1f, 1f);
            historyContentRect.pivot = new Vector2(0.5f, 1f);
            historyContentRect.offsetMin = new Vector2(12f, 0f);
            historyContentRect.offsetMax = new Vector2(-12f, 0f);
            historyContentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layoutGroup = contentObject.GetComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(0, 0, 12, 12);

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            historyScrollRect.viewport = viewportRect;
            historyScrollRect.content = historyContentRect;
            historyScrollRect.verticalScrollbar = historyScrollbar;
            historyScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        private void CreateSettingsField(Transform parent, string label, string fieldName, string placeholder, bool isPassword, int rowIndex, out InputField inputField)
        {
            float topOffset = VerticalPadding + HeaderHeight + 34f + rowIndex * SettingsFieldGap;

            Text labelText = CreateText(parent, label + "Label", 14, TextAnchor.MiddleLeft, new Color(0.32f, 0.36f, 0.42f, 1f));
            SetRect(
                labelText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(HorizontalPadding, -topOffset),
                new Vector2(-HorizontalPadding, -topOffset + 24f));
            labelText.text = label;

            inputField = CreateInputField(parent, fieldName, isPassword, placeholder, 13, InputField.LineType.SingleLine);
            SetRect(
                inputField.GetComponent<RectTransform>(),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(HorizontalPadding, -topOffset - 46f),
                new Vector2(-HorizontalPadding, -topOffset - 46f + SettingsFieldHeight));
        }

        private GameObject CreatePageRoot(string name)
        {
            GameObject page = new GameObject(name, typeof(RectTransform));
            page.transform.SetParent(panelRectTransform, false);

            RectTransform rect = page.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return page;
        }

        private void LoadSavedConfig()
        {
            endpointInput.text = GameSettingsStore.GetAiChatEndpoint();
            apiKeyInput.text = GameSettingsStore.GetAiChatApiKey();
            modelInput.text = GameSettingsStore.GetAiChatModel();
        }

        private void SaveConfig()
        {
            GameSettingsStore.SetAiChatEndpoint(endpointInput.text.Trim());
            GameSettingsStore.SetAiChatApiKey(apiKeyInput.text.Trim());
            GameSettingsStore.SetAiChatModel(modelInput.text.Trim());
            SetSettingsStatus("\u914d\u7f6e\u5df2\u4fdd\u5b58\u3002", false);
            SetChatStatus("\u914d\u7f6e\u5df2\u66f4\u65b0\uff0c\u53ef\u4ee5\u8fd4\u56de\u804a\u5929\u9875\u7ee7\u7eed\u5bf9\u8bdd\u3002", false);
        }

        private void SendMessage()
        {
            if (isSending)
            {
                return;
            }

            if (!HasValidConfig())
            {
                SetChatStatus("\u8bf7\u5148\u5728\u8bbe\u7f6e\u9875\u586b\u5199 URL\u3001API Key \u548c Model\u3002", true);
                ShowSettingsPage();
                return;
            }

            string message = messageInput.text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                SetChatStatus("\u8bf7\u8f93\u5165\u8981\u53d1\u9001\u7684\u5185\u5bb9\u3002", true);
                return;
            }

            AIChatRequestConfig config = new AIChatRequestConfig
            {
                endpoint = endpointInput.text.Trim(),
                apiKey = apiKeyInput.text.Trim(),
                model = modelInput.text.Trim()
            };

            conversation.Add(new AIChatMessageData("user", message));
            AddMessageItem("\u4f60", message, true);
            messageInput.text = string.Empty;
            SetSendingState(true);
            SetChatStatus("\u6b63\u5728\u83b7\u53d6 AI \u56de\u590d...", false);
            StartCoroutine(SendCoroutine(config));
        }

        private IEnumerator SendCoroutine(AIChatRequestConfig config)
        {
            string reply = null;
            string error = null;

            yield return AIChatService.SendChatCompletion(
                config,
                conversation,
                content => reply = content,
                message => error = message);

            SetSendingState(false);

            if (!string.IsNullOrWhiteSpace(error))
            {
                if (conversation.Count > 0 && conversation[conversation.Count - 1].role == "user")
                {
                    conversation.RemoveAt(conversation.Count - 1);
                }

                SetChatStatus(error, true);
                yield break;
            }

            conversation.Add(new AIChatMessageData("assistant", reply));
            AddMessageItem("AI", reply, false);
            SetChatStatus("\u56de\u590d\u5df2\u5237\u65b0\u3002", false);
        }

        private void AddMessageItem(string speaker, string content, bool isUser)
        {
            if (historyContentRect == null)
            {
                return;
            }

            GameObject itemObject = new GameObject("MessageItem", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            itemObject.transform.SetParent(historyContentRect, false);

            Image background = itemObject.GetComponent<Image>();
            background.color = isUser
                ? new Color(0.96f, 0.89f, 0.69f, 1f)
                : new Color(0.93f, 0.95f, 0.99f, 1f);

            RectTransform itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);

            LayoutElement layoutElement = itemObject.GetComponent<LayoutElement>();

            Text messageText = CreateText(itemObject.transform, "MessageText", 14, TextAnchor.UpperLeft, new Color(0.18f, 0.18f, 0.20f, 1f));
            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.verticalOverflow = VerticalWrapMode.Overflow;
            messageText.text = $"{speaker}: {content.Trim()}";

            RectTransform textRect = messageText.rectTransform;
            Stretch(textRect);
            textRect.offsetMin = new Vector2(12f, 10f);
            textRect.offsetMax = new Vector2(-12f, -10f);

            ContentSizeFitter textFitter = messageText.gameObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Canvas.ForceUpdateCanvases();
            float preferredHeight = Mathf.Max(44f, messageText.preferredHeight + 20f);
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minHeight = preferredHeight;

            StartCoroutine(ScrollHistoryToBottomNextFrame());
        }

        private IEnumerator ScrollHistoryToBottomNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyContentRect);
            historyScrollRect.verticalNormalizedPosition = 0f;
            historyScrollbar.value = 0f;
        }

        private void SetSendingState(bool sending)
        {
            isSending = sending;

            if (sendButton != null)
            {
                sendButton.interactable = !sending;
            }

            if (saveButton != null)
            {
                saveButton.interactable = !sending;
            }

            if (messageInput != null)
            {
                messageInput.interactable = !sending;
            }
        }

        private bool HasValidConfig()
        {
            return !string.IsNullOrWhiteSpace(endpointInput.text) &&
                   !string.IsNullOrWhiteSpace(apiKeyInput.text) &&
                   !string.IsNullOrWhiteSpace(modelInput.text);
        }

        private void ShowChatPage()
        {
            if (chatPage != null)
            {
                chatPage.SetActive(true);
            }

            if (settingsPage != null)
            {
                settingsPage.SetActive(false);
            }
        }

        private void ShowSettingsPage()
        {
            if (chatPage != null)
            {
                chatPage.SetActive(false);
            }

            if (settingsPage != null)
            {
                settingsPage.SetActive(true);
            }
        }

        private void SetChatStatus(string message, bool isError)
        {
            if (chatStatusText == null)
            {
                return;
            }

            chatStatusText.text = message;
            chatStatusText.color = isError ? new Color(0.77f, 0.18f, 0.16f, 1f) : new Color(0.24f, 0.31f, 0.40f, 1f);
        }

        private void SetSettingsStatus(string message, bool isError)
        {
            if (settingsStatusText == null)
            {
                return;
            }

            settingsStatusText.text = message;
            settingsStatusText.color = isError ? new Color(0.77f, 0.18f, 0.16f, 1f) : new Color(0.24f, 0.31f, 0.40f, 1f);
        }

        private void Close()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            ownerManager?.NotifyAiChatPanelClosed(this);
        }

        private static Text CreateText(Transform parent, string name, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);

            Text text = obj.GetComponent<Text>();
            text.font = GetFont();
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static InputField CreateInputField(Transform parent, string name, bool isPassword, string placeholderText, int fontSize, InputField.LineType lineType)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            root.transform.SetParent(parent, false);

            Image background = root.GetComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.95f);

            InputField inputField = root.GetComponent<InputField>();
            inputField.contentType = isPassword ? InputField.ContentType.Password : InputField.ContentType.Standard;
            inputField.lineType = lineType;
            inputField.targetGraphic = background;

            Text textComponent = CreateText(root.transform, "Text", fontSize, TextAnchor.UpperLeft, new Color(0.18f, 0.18f, 0.20f, 1f));
            textComponent.supportRichText = false;
            textComponent.raycastTarget = true;
            RectTransform textRect = textComponent.rectTransform;
            Stretch(textRect);
            textRect.offsetMin = new Vector2(10f, 8f);
            textRect.offsetMax = new Vector2(-10f, -8f);

            Text placeholder = CreateText(root.transform, "Placeholder", fontSize, TextAnchor.UpperLeft, new Color(0.52f, 0.57f, 0.63f, 0.9f));
            placeholder.text = placeholderText;
            RectTransform placeholderRect = placeholder.rectTransform;
            Stretch(placeholderRect);
            placeholderRect.offsetMin = new Vector2(10f, 8f);
            placeholderRect.offsetMax = new Vector2(-10f, -8f);

            inputField.textComponent = textComponent;
            inputField.placeholder = placeholder;
            return inputField;
        }

        private static Button CreateButton(Transform parent, string name, string label, int fontSize)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.85f, 0.70f, 0.28f, 1f);

            Button button = root.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = background.color;
            colors.highlightedColor = new Color(0.92f, 0.77f, 0.34f, 1f);
            colors.pressedColor = new Color(0.73f, 0.58f, 0.18f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.70f, 0.67f, 0.58f, 0.8f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            Text labelText = CreateText(root.transform, "Label", fontSize, TextAnchor.MiddleCenter, new Color(0.27f, 0.21f, 0.11f, 1f));
            labelText.text = label;
            Stretch(labelText.rectTransform);
            return button;
        }

        private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static Font GetFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }
    }
}
