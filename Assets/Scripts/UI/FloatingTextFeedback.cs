using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    /// <summary>
    /// Displays a temporary non-blocking UI text that floats upward and fades out.
    /// </summary>
    public class FloatingTextFeedback : MonoBehaviour
    {
        private const float Lifetime = 1.15f;
        private const float FloatDistance = 28f;

        private Text text;
        private RectTransform rect;
        private Vector2 startPosition;
        private float elapsed;

        /// <summary>
        /// Creates and displays a floating text feedback element.
        /// </summary>
        /// <param name="parent">The UI transform that owns the feedback element.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="anchoredPosition">The starting anchored position relative to the parent.</param>
        /// <param name="color">The text color.</param>
        public static void Show(Transform parent, string message, Vector2 anchoredPosition, Color color)
        {
            GameObject obj = new("FloatingTextFeedback", typeof(RectTransform), typeof(Text), typeof(FloatingTextFeedback));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(160f, 28f);

            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.text = message;
            text.fontSize = 15;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            FloatingTextFeedback feedback = obj.GetComponent<FloatingTextFeedback>();
            feedback.text = text;
            feedback.rect = rect;
            feedback.startPosition = anchoredPosition;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Lifetime);
            rect.anchoredPosition = startPosition + new Vector2(0f, FloatDistance * t);

            Color color = text.color;
            color.a = 1f - t;
            text.color = color;

            if (elapsed >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
