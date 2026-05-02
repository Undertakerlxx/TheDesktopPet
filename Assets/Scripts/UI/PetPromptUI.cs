using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class PetPromptUI : MonoBehaviour
    {
        private const float PromptVerticalOffset = 8f;
        private const float PromptHeight = 28f;
        private const float PromptMinWidth = 240f;

        public RectTransform panel;
        public Text promptText;

        private float hideAtTime;

        private void Awake()
        {
            EnsurePromptText();
        }

        private void Update()
        {
            if (promptText == null || !promptText.gameObject.activeSelf)
            {
                return;
            }

            if (Time.time >= hideAtTime)
            {
                promptText.gameObject.SetActive(false);
                promptText.text = string.Empty;
            }
        }

        public void ShowPrompt(string message, float duration)
        {
            EnsurePromptText();
            if (promptText == null)
            {
                return;
            }

            promptText.text = message;
            promptText.gameObject.SetActive(true);
            hideAtTime = Time.time + Mathf.Max(0f, duration);
        }

        private void EnsurePromptText()
        {
            if (panel == null)
            {
                panel = GetComponent<RectTransform>();
            }

            if (promptText == null && panel != null)
            {
                Transform promptTransform = panel.Find("PromptText");
                if (promptTransform != null)
                {
                    promptText = promptTransform.GetComponent<Text>();
                }
            }

            if (promptText == null && panel != null)
            {
                promptText = CreatePromptText();
            }

            if (promptText != null)
            {
                ConfigurePromptText(promptText);
            }
        }

        private Text CreatePromptText()
        {
            GameObject promptObject = new("PromptText", typeof(RectTransform), typeof(Text));
            promptObject.transform.SetParent(panel, false);
            promptObject.SetActive(false);

            return promptObject.GetComponent<Text>();
        }

        private void ConfigurePromptText(Text prompt)
        {
            if (panel == null || prompt == null)
            {
                return;
            }

            RectTransform promptRect = prompt.rectTransform;
            promptRect.anchorMin = new Vector2(0.5f, 1f);
            promptRect.anchorMax = new Vector2(0.5f, 1f);
            promptRect.pivot = new Vector2(0.5f, 0f);
            promptRect.anchoredPosition = new Vector2(0f, PromptVerticalOffset);
            promptRect.sizeDelta = new Vector2(Mathf.Max(PromptMinWidth, panel.rect.width), PromptHeight);

            Text referenceText = GetReferenceText();
            if (referenceText != null)
            {
                prompt.font = referenceText.font;
                prompt.fontSize = referenceText.fontSize;
                prompt.fontStyle = referenceText.fontStyle;
                prompt.lineSpacing = referenceText.lineSpacing;
            }
            else
            {
                prompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                prompt.fontSize = 16;
                prompt.fontStyle = FontStyle.Normal;
            }

            prompt.alignment = TextAnchor.MiddleCenter;
            prompt.color = new Color(0.94f, 0.35f, 0.25f, 1f);
            prompt.horizontalOverflow = HorizontalWrapMode.Overflow;
            prompt.verticalOverflow = VerticalWrapMode.Overflow;
            prompt.raycastTarget = false;
        }

        private Text GetReferenceText()
        {
            PetStatsDisplayUI statsDisplay = GetComponent<PetStatsDisplayUI>();
            if (statsDisplay != null && statsDisplay.statsText != null && statsDisplay.statsText != promptText)
            {
                return statsDisplay.statsText;
            }

            Text[] textComponents = GetComponentsInChildren<Text>(true);
            foreach (Text textComponent in textComponents)
            {
                if (textComponent != null && textComponent != promptText)
                {
                    return textComponent;
                }
            }

            return null;
        }
    }
}
